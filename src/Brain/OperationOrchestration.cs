namespace Autoccultist.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Assets.CS.TabletopUI;
    using Assets.TabletopUi;
    using Autoccultist.Actor;
    using Autoccultist.Actor.Actions;

    /// <summary>
    /// An orchestration of a situation that executes an operation.
    /// </summary>
    public class OperationOrchestration : ISituationOrchestration
    {
        private readonly IOperation operation;

        private CancellationTokenSource cancelCurrentTask;

        private OperationState operationState = OperationState.Unstarted;

        private string ongoingRecipe;

        private DateTime? completionDebounceTime = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationOrchestration"/> class.
        /// </summary>
        /// <param name="operation">The operation for this orchestration to execute.</param>
        public OperationOrchestration(IOperation operation)
        {
            this.operation = operation;
        }

        /// <inheritdoc/>
        public event EventHandler Completed;

        /// <summary>
        /// The state of the operation being executed.
        /// </summary>
        private enum OperationState
        {
            /// <summary>
            /// The operation has yet to start.
            /// </summary>
            Unstarted,

            /// <summary>
            /// The operation is filling out its cards and starting up.
            /// </summary>
            Starting,

            /// <summary>
            /// The operation is performing its recipes and waiting on the result.
            /// </summary>
            Ongoing,

            /// <summary>
            /// The operation is choosing a card in the mansus
            /// </summary>
            Mansus,

            /// <summary>
            /// The operation has finished and we are waiting for the contents to dump.
            /// </summary>
            Completing,

            /// <summary>
            /// The operation was aborted.
            /// </summary>
            Finished,
        }

        /// <summary>
        /// Gets or sets the time delay to check if a situation is really completed, or just transitioning between two recipes.
        /// </summary>
        public static TimeSpan CompleteAwaitTime { get; set; } = TimeSpan.FromSeconds(0.2);

        /// <inheritdoc/>
        public string SituationId
        {
            get
            {
                return this.operation.Situation;
            }
        }

        private SituationController Situation
        {
            get
            {
                var situation = GameAPI.GetSituation(this.SituationId);
                if (situation == null)
                {
                    AutoccultistPlugin.Instance.LogWarn($"Cannot start solution - Situation {this.SituationId} not found.");
                }

                return situation;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[OperationOrchestration Operation={this.operation.Name}]";
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (this.operationState != OperationState.Unstarted)
            {
                return;
            }

            var situation = this.Situation;
            if (situation == null)
            {
                throw new Exception("Tried to start a situation solution with no situation.");
            }

            this.operationState = OperationState.Starting;
            this.RunCoroutine(this.StartOperationCoroutine());
        }

        /// <inheritdoc/>
        public void Update()
        {
            if (this.operationState == OperationState.Unstarted)
            {
                return;
            }

            if (this.operationState == OperationState.Ongoing)
            {
                var clockState = this.Situation.SituationClock.State;
                if (clockState == SituationState.Ongoing)
                {
                    // The situation is running its recipe.
                    //  Reset our state in case we were tracking a previous SituationClock.State == Complete.
                    this.completionDebounceTime = null;

                    // See if we need to slot new cards.
                    this.ContinueOperation();
                }
                else if (clockState == SituationState.Complete || clockState == SituationState.Unstarted)
                {
                    if (this.completionDebounceTime == null)
                    {
                        // Start the timer for waiting out to see if this is a real completion.
                        // Situations may tenatively say Complete when they are transitioning to a continuation recipe,
                        //  so we need to delay to make sure this is a full and final completion.
                        this.completionDebounceTime = DateTime.Now + CompleteAwaitTime;
                    }
                    else if (this.completionDebounceTime <= DateTime.Now)
                    {
                        // Enough time has passed that we can consider this operation completed.
                        this.Complete();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Abort()
        {
            this.cancelCurrentTask?.Cancel();
            BrainEventSink.OnOperationAborted(this.operation);
            this.End();
        }

        private async void Complete()
        {
            // We currently assume mansus only ever occurs as the last recipe of a situation.
            var recipeId = this.Situation.SituationClock.RecipeId;
            var recipe = GameAPI.GetRecipe(recipeId);
            if (recipe.PortalEffect != PortalEffect.None && GameAPI.IsInMansus)
            {
                if (this.operation.OngoingRecipes.TryGetValue(recipeId, out var recipeSolution) && recipeSolution.MansusChoice != null)
                {
                    this.operationState = OperationState.Mansus;
                    await this.AwaitCoroutine(this.ChooseMansusCoroutine(recipeSolution.MansusChoice));
                }
                else
                {
                    AutoccultistPlugin.Instance.LogWarn($"Unhandled mansus event for recipeId {recipeId} in operation {this.operation.Name}.");
                }
            }

            this.operationState = OperationState.Completing;
            await this.AwaitCoroutine(this.CompleteOperationCoroutine());
        }

        private void ContinueOperation()
        {
            var currentRecipe = this.Situation.SituationClock.RecipeId;
            if (this.ongoingRecipe == currentRecipe)
            {
                // Recipe has not changed.
                return;
            }

            this.ongoingRecipe = currentRecipe;

            if (this.operation.OngoingRecipes == null)
            {
                // No recipes to continue
                return;
            }

            if (!this.operation.OngoingRecipes.TryGetValue(currentRecipe, out var recipeSolution))
            {
                // Operation does not know this recipe.
                return;
            }

            this.RunCoroutine(this.ContinueSituationCoroutine(recipeSolution));
        }

        private IEnumerable<IAutoccultistAction> StartOperationCoroutine()
        {
            BrainEventSink.OnOperationStarted(this.operation);

            yield return new OpenSituationAction(this.SituationId);
            yield return new DumpSituationAction(this.SituationId);

            // TODO: Use reserved cards

            // Get the first card.  Slotting this will usually create additional slots
            var slots = this.Situation.situationWindow.GetStartingSlots();
            var firstSlot = slots[0];
            var firstSlotAction = this.CreateSlotActionFromRecipe(firstSlot, this.operation.StartingRecipe);
            if (firstSlotAction == null)
            {
                // First slot of starting situation is required.
                throw new OperationFailedException($"Error in operation {this.operation.Name}: Slot id {firstSlot.GoverningSlotSpecification.Id} has no card choice.");
            }

            yield return firstSlotAction;

            // Refresh the slots and get the rest of the cards.
            slots = this.Situation.situationWindow.GetStartingSlots();
            foreach (var slot in slots.Skip(1))
            {
                var slotAction = this.CreateSlotActionFromRecipe(slot, this.operation.StartingRecipe);
                if (slotAction != null)
                {
                    yield return slotAction;
                }
            }

            // Start the situation
            yield return new StartSituationRecipeAction(this.SituationId);

            // Mark us as ongoing now that we started the recipe.
            this.operationState = OperationState.Ongoing;

            // Accept the current recipe and fill its needs
            this.ongoingRecipe = this.Situation.SituationClock.RecipeId;
            if (this.operation.OngoingRecipes != null && this.operation.OngoingRecipes.TryGetValue(this.ongoingRecipe, out var ongoingRecipeSolution))
            {
                foreach (var item in this.ContinueSituationCoroutine(ongoingRecipeSolution, false))
                {
                    yield return item;
                }
            }

            yield return new CloseSituationAction(this.SituationId);
        }

        private IEnumerable<IAutoccultistAction> ContinueSituationCoroutine(IRecipeSolution recipe, bool standalone = true)
        {
            var slots = this.Situation.situationWindow.GetOngoingSlots();
            if (slots.Count == 0)
            {
                // Nothing to do.
                yield break;
            }

            var firstSlot = slots[0];

            if (firstSlot.GetTokenInSlot() != null)
            {
                // Something is already slotted in, we already handled this.
                yield break;
            }

            if (standalone)
            {
                yield return new OpenSituationAction(this.SituationId);
            }

            // Get the first card.  Slotting this will usually create additional slots
            var firstSlotAction = this.CreateSlotActionFromRecipe(firstSlot, recipe);
            if (firstSlotAction == null)
            {
                // Not sure if the first slot of ongoing actions is always required...
                throw new OperationFailedException($"Error in operation {this.operation.Name}: Slot id {firstSlot.GoverningSlotSpecification.Id} has no card choice.");
            }

            yield return firstSlotAction;

            // Refresh the slots and get the rest of the cards
            slots = this.Situation.situationWindow.GetOngoingSlots();
            foreach (var slot in slots.Skip(1))
            {
                var slotAction = this.CreateSlotActionFromRecipe(slot, recipe);
                if (slotAction != null)
                {
                    yield return slotAction;
                }
            }

            if (standalone)
            {
                yield return new CloseSituationAction(this.SituationId);
            }
        }

        private IEnumerable<IAutoccultistAction> ChooseMansusCoroutine(IMansusSolution solution)
        {
            yield return new ChooseMansusCardAction(solution);
        }

        private IEnumerable<IAutoccultistAction> CompleteOperationCoroutine()
        {
            try
            {
                yield return new OpenSituationAction(this.SituationId);
                yield return new DumpSituationAction(this.SituationId);
                yield return new CloseSituationAction(this.SituationId);
            }
            finally
            {
                BrainEventSink.OnOperationCompleted(this.operation);
                this.End();
            }
        }

        private SlotCardAction CreateSlotActionFromRecipe(RecipeSlot slot, IRecipeSolution recipe)
        {
            var slotId = slot.GoverningSlotSpecification.Id;
            var cardChoice = recipe.ResolveSlotCard(slot);
            if (cardChoice == null)
            {
                return null;
            }

            return new SlotCardAction(this.SituationId, slotId, cardChoice);
        }

        private async void RunCoroutine(IEnumerable<IAutoccultistAction> coroutine)
        {
            await this.AwaitCoroutine(coroutine);
        }

        private async Task AwaitCoroutine(IEnumerable<IAutoccultistAction> coroutine)
        {
            // Note: PerformActions gives us a Task that crawls along on the main thread.
            //  Because of this, Waiting this task will create a deadlock.
            // Async functions are fine, as they are broken up into state machines with Continue
            try
            {
                this.cancelCurrentTask = new CancellationTokenSource();
                await AutoccultistActor.PerformActions(coroutine, this.cancelCurrentTask.Token);
            }
            catch (Exception ex)
            {
                AutoccultistPlugin.Instance.LogWarn($"Failed to run operation {this.operation.Name}: {ex.Message}");
                this.Abort();
            }
            finally
            {
                this.cancelCurrentTask = null;
            }
        }

        private void End()
        {
            this.operationState = OperationState.Finished;
            this.Completed?.Invoke(this, EventArgs.Empty);
        }
    }
}
