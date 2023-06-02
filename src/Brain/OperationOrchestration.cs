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
            ChoosingPortalCard,

            /// <summary>
            /// A mansus card has been chosen and we are waiting to dump the portal result.
            /// </summary>
            AwaitingPortalResults,

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
                    var recipeSolution = this.operation.GetOngoingRecipeSolution(situation);
                    if (recipeSolution == null)
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

            if (this.operationState == OperationState.Ongoing)
            {
                this.UpdateOngoing();
            }
            else if (this.operationState == OperationState.AwaitingPortalResults)
            {
                this.UpdateAwaitPortalResults();
            }
        }

        /// <inheritdoc/>
        public void Abort()
        {
            // TODO: We might want to clean up, dump the contents and close the window.
            // Doing so is tricky if we want to use the actor though, as it means the abort action is asynchronous.
            this.cancelCurrentTask?.Cancel();
            this.End(true);
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

            Autoccultist.Instance.LogTrace($"Completing operation {this.operation.Name} on recipe {situation.CurrentRecipe}.");

            // We currently assume mansus only ever occurs as the last recipe of a situation.
            // FIXME: This is quite janky...  We just hope the portal manages to open by the time we notice completion.
            // Hopefully it opens instantaniously before we get to it.
            // We dont even know this is our portal, although we can guess by checking to see if our situation has a portal assigned to it.
            if (situation.CurrentRecipePortal != null && GameStateProvider.Current.Mansus.State != PortalActiveState.Closed)
            {
                var recipeSolution = this.operation.GetOngoingRecipeSolution(situation);
                if (recipeSolution != null && recipeSolution.MansusChoice != null)
                {
                    Autoccultist.Instance.LogTrace($"Choosing mansus card for operation {this.operation.Name} portal {situation.CurrentRecipePortal}.");
                    this.operationState = OperationState.ChoosingPortalCard;
                    await this.AwaitCoroutine(this.ChooseMansusCoroutine(recipeSolution.MansusChoice));
                    return;
                }
                else
                {
                    Autoccultist.Instance.LogWarn($"Unhandled mansus event for recipeId {situation.CurrentRecipe} in operation {this.operation.Name}.");
                }
            }

            this.operationState = OperationState.Completing;
            await this.AwaitCoroutine(this.CompleteOperationCoroutine());
        }

        private void UpdateOngoing()
        {
            var situation = this.GetSituationState();
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

        private async void UpdateAwaitPortalResults()
        {
            var state = GameStateProvider.Current;

            if (state.Mansus.State != PortalActiveState.AwaitingCollection)
            {
                return;
            }

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

            Autoccultist.Instance.LogTrace($"Continuing operation {this.operation.Name}.  From recipe {this.ongoingRecipe} to {currentRecipe}.");

            this.ongoingRecipe = currentRecipe;
            this.ongoingRecipeTimeRemaining = situation.RecipeTimeRemaining ?? 0;

            var recipeSolution = this.operation.GetOngoingRecipeSolution(situation);

            if (recipeSolution == null)
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
            yield return new EmptySituationAction(this.SituationId);

            var recipeSolution = this.operation.StartingRecipe;

            if (recipeSolution == null)
            {
                throw new OperationFailedException($"Error in operation {this.operation.Name}: No starting recipe defined.");
            }

            var populatedSlots = new HashSet<string>();

            // Get the first card.  Slotting this will usually create additional slots
            var slots = this.GetSituationState().RecipeSlots;
            var firstSlot = slots.First();
            var firstSlotSpecId = firstSlot.SpecId;

            var firstSlotAction = this.GetSlotActionForRecipeSlotSpec(firstSlotSpecId, recipeSolution);
            if (firstSlotAction == null)
            {
                // First slot of starting situation is required.
                throw new OperationFailedException($"Error in operation {this.operation.Name}: Slot id {firstSlotSpecId} has no card choice.");
            }

            populatedSlots.Add(firstSlotSpecId);
            yield return firstSlotAction;

            // Refresh the slots and get the rest of the cards.
            // We need to capture the value with ToArray, as state will not be valid after we perform an action.
            // It is safe to assume our situation will mantain the same slots, as the first slot has already been populated.
            var slotsSpecIds = this.GetSituationState().RecipeSlots.Select(x => x.SpecId).Where(x => x != firstSlotSpecId).ToArray();
            foreach (var slotSpecId in slotsSpecIds)
            {
                var slotAction = this.GetSlotActionForRecipeSlotSpec(slotSpecId, recipeSolution);
                if (slotAction != null)
                {
                    populatedSlots.Add(slotSpecId);
                    yield return slotAction;
                }
                else
                {
                    Autoccultist.Instance.LogTrace($"Operation {this.operation.Name} has no starting slot matcher for {slotSpecId}.");
                }
            }

            // Check that all required slots were populated.
            var missingSlots = recipeSolution.SlotSolutions.Keys.Except(populatedSlots);
            if (missingSlots.Any())
            {
                Autoccultist.Instance.LogTrace($"Operation {this.operation.Name} did not define starting recipe slots for {string.Join(", ", missingSlots)}.");
            }

            // Start the situation
            yield return new StartSituationRecipeAction(this.SituationId);

            if (recipeSolution.EndOperation)
            {
                yield return new CloseSituationAction(this.SituationId);
                this.End();
            }

            // Mark us as ongoing now that we started the recipe.
            this.operationState = OperationState.Ongoing;

            // Accept the current recipe and fill its needs
            var followupRecipeSolution = this.operation.GetOngoingRecipeSolution(this.GetSituationState());
            if (followupRecipeSolution != null)
            {
                foreach (var item in this.ContinueSituationCoroutine(followupRecipeSolution, false))
                {
                    yield return item;
                }
            }

            yield return new CloseSituationAction(this.SituationId);


            if (followupRecipeSolution?.EndOperation == true)
            {
                this.End();
            }
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
            var firstSlotSpecId = firstSlot.SpecId;

            if (firstSlot.Card != null)
            {
                // Something is already slotted in, we already handled this.
                yield break;
            }

            if (standalone)
            {
                yield return new OpenSituationAction(this.SituationId);
            }

            var populatedSlots = new HashSet<string>();

            // Get the first card.  Slotting this will usually create additional slots
            var firstSlotAction = this.GetSlotActionForRecipeSlotSpec(firstSlotSpecId, recipe);
            if (firstSlotAction != null)
            {
                populatedSlots.Add(firstSlotSpecId);
                yield return firstSlotAction;
            }

            // Slotting the first spec can change the recipe, changing the available slots.
            // Refresh the slots and get the rest of the cards.
            // We need to get the IDs here, as the slot state references will be stale after our first action.
            // Capture the value with ToArray as the state will be invalidated when we perform an action.
            var slotSpecIds = this.GetSituationState().RecipeSlots.Select(x => x.SpecId).Where(x => x != firstSlotSpecId).ToArray();
            foreach (var slotSpecId in slotSpecIds)
            {
                var slotAction = this.GetSlotActionForRecipeSlotSpec(slotSpecId, recipe);
                if (slotAction != null)
                {
                    populatedSlots.Add(slotSpecId);
                    yield return slotAction;
                }
            }

            // Check that all required slots were populated.
            var missingSlots = recipe.SlotSolutions.Keys.Except(populatedSlots);
            if (missingSlots.Any())
            {
                Autoccultist.Instance.LogTrace($"Operation {this.operation.Name} recipe {this.ongoingRecipe} did not define recipe slots for {string.Join(", ", missingSlots)}.");
            }

            if (standalone)
            {
                yield return new CloseSituationAction(this.SituationId);
            }

            if (recipe.EndOperation)
            {
                this.End();
            }
        }

        private IEnumerable<IAutoccultistAction> ChooseMansusCoroutine(IMansusSolution solution)
        {
            yield return new ChooseMansusCardAction(solution);
            this.operationState = OperationState.AwaitingPortalResults;
        }

        private IEnumerable<IAutoccultistAction> CompleteOperationCoroutine(bool acceptMansus = false)
        {
            try
            {
                yield return new AcceptMansusResultsAction();
                yield return new OpenSituationAction(this.SituationId);
                yield return new EmptySituationAction(this.SituationId);
                yield return new CloseSituationAction(this.SituationId);
            }
            finally
            {
                this.End();
            }
        }

        private SlotCardAction GetSlotActionForRecipeSlotSpec(string specId, IRecipeSolution recipe)
        {
            if (!recipe.SlotSolutions.TryGetValue(specId, out var cardChoice))
            {
                return null;
            }

            return new SlotCardAction(this.SituationId, specId, cardChoice);
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
                Autoccultist.Instance.LogWarn(ex, $"Failed to run operation {this.operation.Name}: {ex.Message}");
                this.Abort();
            }
            finally
            {
                this.cancelCurrentTask = null;
            }
        }

        private void End(bool aborted = false)
        {
            if (aborted)
            {
                BrainEventSink.OnOperationAborted(this.operation);
            }
            else
            {
                BrainEventSink.OnOperationCompleted(this.operation);
            }

            this.operationState = OperationState.Finished;
            this.Completed?.Invoke(this, EventArgs.Empty);
        }
    }
}
