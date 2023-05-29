namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Actor;
    using AutoccultistNS.Actor.Actions;
    using AutoccultistNS.GameState;
    using SecretHistories.Enums;

    /// <summary>
    /// An orchestration of a situation that executes an operation.
    /// </summary>
    public class OperationOrchestration : ISituationOrchestration
    {
        private readonly IOperation operation;

        private CancellationTokenSource cancelCurrentTask;

        private OperationState operationState = OperationState.Unstarted;

        private string ongoingRecipe;
        private float ongoingRecipeTimeRemaining;

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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[OperationOrchestration Operation=\"{this.operation.Name}\" State={this.operationState}]";
        }

        /// <inheritdoc/>
        public void Start()
        {
            var situation = this.GetSituationState();

            if (this.operationState != OperationState.Unstarted)
            {
                return;
            }

            switch (situation.State)
            {
                case StateEnum.Unstarted:
                case StateEnum.RequiringExecution:
                    this.operationState = OperationState.Starting;
                    this.RunCoroutine(this.StartOperationCoroutine());
                    break;
                case StateEnum.Ongoing:
                    this.operationState = OperationState.Ongoing;
                    if (!this.operation.OngoingRecipes.TryGetValue(situation.CurrentRecipe, out var recipeSolution))
                    {
                        // Operation does not know this recipe.
                        return;
                    }

                    this.RunCoroutine(this.ContinueSituationCoroutine(recipeSolution));
                    break;
                default:
                    // This happened to a the cult activity that should have been ongoing...  Got stuck on an unstarted state.
                    NoonUtility.LogWarning($"Cannot start solution - Situation {this.SituationId} is in state {situation.State}.");
                    this.Abort();
                    break;
            }
        }

        /// <inheritdoc/>
        public void Update()
        {
            var situation = this.GetSituationState();

            if (this.operationState != OperationState.Ongoing)
            {
                return;
            }

            var clockState = situation.State;
            if (clockState == StateEnum.Ongoing)
            {
                // The situation is running its recipe.
                //  Reset our state in case we were tracking a previous SituationClock.State == Complete.
                this.completionDebounceTime = null;

                // See if we need to slot new cards.
                this.ContinueOperation();
            }
            else if (clockState == StateEnum.Complete || clockState == StateEnum.Unstarted)
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

        /// <inheritdoc/>
        public void Abort()
        {
            // TODO: We might want to clean up, dump the contents and close the window.
            // Doing so is tricky if we want to use the actor though, as it means the abort action is asynchronous.
            this.cancelCurrentTask?.Cancel();
            BrainEventSink.OnOperationAborted(this.operation);
            this.End();
        }

        private ISituationState GetSituationState()
        {
            var situation = GameStateProvider.Current.Situations.FirstOrDefault(x => x.SituationId == this.SituationId);
            if (situation == null)
            {
                throw new Exception($"Tried to run an operation orchistration for situation {this.SituationId}, but no situation was found.");
            }

            return situation;
        }

        private async void Complete()
        {
            var situation = this.GetSituationState();

            // We currently assume mansus only ever occurs as the last recipe of a situation.
            // FIXME: Engine supports multiple portal / otherworld / mansus types, but we just assume the one.
            if (situation.CurrentRecipePortal != null && GameStateProvider.Current.Mansus.IsActive)
            {
                if (this.operation.OngoingRecipes.TryGetValue(situation.CurrentRecipe, out var recipeSolution) && recipeSolution.MansusChoice != null)
                {
                    this.operationState = OperationState.Mansus;
                    await this.AwaitCoroutine(this.ChooseMansusCoroutine(recipeSolution.MansusChoice));
                }
                else
                {
                    NoonUtility.LogWarning($"Unhandled mansus event for recipeId {situation.CurrentRecipe} in operation {this.operation.Name}.");
                }
            }

            this.operationState = OperationState.Completing;
            await this.AwaitCoroutine(this.CompleteOperationCoroutine());
        }

        private void ContinueOperation()
        {
            var situation = this.GetSituationState();

            var currentRecipe = situation.CurrentRecipe;

            // We need to check the time remaining, because a recipe can repeat.
            if (this.ongoingRecipe == currentRecipe && situation.RecipeTimeRemaining <= this.ongoingRecipeTimeRemaining)
            {
                // Recipe has not changed.
                this.ongoingRecipeTimeRemaining = situation.RecipeTimeRemaining.Value;
                return;
            }

            NoonUtility.LogWarning($"Continuing operation {this.operation.Name}.  From recipe {this.ongoingRecipe} to {currentRecipe}.");

            this.ongoingRecipe = currentRecipe;
            this.ongoingRecipeTimeRemaining = situation.RecipeTimeRemaining ?? 0;

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
            yield return new ConcludeSituationAction(this.SituationId);

            Autoccultist.Instance.LogTrace($"Our known slots are {string.Join(", ", this.operation.StartingRecipe.SlotSolutions.Keys)}");

            var populatedSlots = new HashSet<string>();

            // Get the first card.  Slotting this will usually create additional slots
            var slots = this.GetSituationState().RecipeSlots;
            var firstSlot = slots.First();
            var firstSlotSpecId = firstSlot.SpecId;
            Autoccultist.Instance.LogTrace($"First slot is {firstSlot.SpecId}.");

            var firstSlotAction = this.GetSlotActionForRecipeSlotSpec(firstSlot, this.operation.StartingRecipe);
            if (firstSlotAction == null)
            {
                Autoccultist.Instance.LogTrace($"We did not find a card for {firstSlot.SpecId}.");
                // First slot of starting situation is required.
                throw new OperationFailedException($"Error in operation {this.operation.Name}: Slot id {firstSlot.SpecId} has no card choice.");
            }

            Autoccultist.Instance.LogTrace($"We found a card for first slot {firstSlot.SpecId}.");

            populatedSlots.Add(firstSlot.SpecId);
            yield return firstSlotAction;

            Autoccultist.Instance.LogTrace("Continuing with remaining slots.");

            // Refresh the slots and get the rest of the cards.
            // We need to fetch the state fresh, as this code continues after the first slot was triggered.
            slots = this.GetSituationState().RecipeSlots;
            Autoccultist.Instance.LogTrace($"We have {slots.Count} slots: {string.Join(", ", slots.Select(x => x.SpecId))}.");
            foreach (var slot in slots.Where(x => x.SpecId != firstSlotSpecId))
            {
                Autoccultist.Instance.LogTrace($"Additional slot is {slot.SpecId}.");
                var slotAction = this.GetSlotActionForRecipeSlotSpec(slot, this.operation.StartingRecipe);
                if (slotAction != null)
                {
                    Autoccultist.Instance.LogTrace($"We found a card for additional slot {slot.SpecId}.");
                    populatedSlots.Add(slot.SpecId);
                    yield return slotAction;
                    Autoccultist.Instance.LogTrace($"We slotted a card for additional slot {slot.SpecId}.");
                }
                else
                {
                    Autoccultist.Instance.LogTrace($"We did not find a card for {slot.SpecId}.");
                }
            }

            // Check that all required slots were populated.
            var missingSlots = this.operation.StartingRecipe.SlotSolutions.Keys.Except(populatedSlots);
            if (missingSlots.Any())
            {
                Autoccultist.Instance.LogTrace($"We are missing starting recipe slots {string.Join(", ", missingSlots)}.");
            }

            Autoccultist.Instance.LogTrace($"Starting situation.");

            // Start the situation
            yield return new StartSituationRecipeAction(this.SituationId);

            // Mark us as ongoing now that we started the recipe.
            this.operationState = OperationState.Ongoing;

            // Accept the current recipe and fill its needs
            this.ongoingRecipe = this.GetSituationState().CurrentRecipe;
            Autoccultist.Instance.LogTrace($"Transitioned to recipe {this.ongoingRecipe}.");
            if (this.operation.OngoingRecipes != null && this.operation.OngoingRecipes.TryGetValue(this.ongoingRecipe, out var ongoingRecipeSolution))
            {
                foreach (var item in this.ContinueSituationCoroutine(ongoingRecipeSolution, false))
                {
                    yield return item;
                }
            }

            Autoccultist.Instance.LogTrace($"Closing situation.");
            yield return new CloseSituationAction(this.SituationId);
        }

        private IEnumerable<IAutoccultistAction> ContinueSituationCoroutine(IRecipeSolution recipe, bool standalone = true)
        {
            var slots = this.GetSituationState().RecipeSlots;
            if (slots.Count == 0)
            {
                // Nothing to do.
                yield break;
            }

            var firstSlot = slots.First();

            if (firstSlot.Card != null)
            {
                // Something is already slotted in, we already handled this.
                yield break;
            }

            if (standalone)
            {
                yield return new OpenSituationAction(this.SituationId);
            }

            // Get the first card.  Slotting this will usually create additional slots
            var firstSlotAction = this.GetSlotActionForRecipeSlotSpec(firstSlot, recipe);
            if (firstSlotAction != null)
            {
                yield return firstSlotAction;

                // Slotting the first spec can change the recipe, changing the available slots.
                // Refresh the slots and get the rest of the cards
                slots = this.GetSituationState().RecipeSlots;
            }

            foreach (var slot in slots.Skip(1))
            {
                var slotAction = this.GetSlotActionForRecipeSlotSpec(slot, recipe);
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
                yield return new ConcludeSituationAction(this.SituationId);
                yield return new CloseSituationAction(this.SituationId);
            }
            finally
            {
                BrainEventSink.OnOperationCompleted(this.operation);
                this.End();
            }
        }

        private SlotCardAction GetSlotActionForRecipeSlotSpec(ISituationSlot slot, IRecipeSolution recipe)
        {
            if (!recipe.SlotSolutions.TryGetValue(slot.SpecId, out var cardChoice))
            {
                NoonUtility.LogWarning($"Error in operation {this.operation.Name}: Slot id {slot.SpecId} has no card choice.");
                return null;
            }

            return new SlotCardAction(this.SituationId, slot.SpecId, cardChoice);
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
                NoonUtility.LogWarning(ex, $"Failed to run operation {this.operation.Name}: {ex.Message}");
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
