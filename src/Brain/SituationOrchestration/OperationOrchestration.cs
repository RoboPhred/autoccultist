namespace AutoccultistNS.Brain
{
    using System;
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

        private Task currentCoroutine;
        private string currentCoroutineName;
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
        public enum OperationState
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
            /// The operation is waiting for the current recipe to complete before detatching.
            /// </summary>
            Orphaning,

            /// <summary>
            /// The operation is choosing a card in the mansus
            /// </summary>
            ChoosingPortalCard,

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

        public OperationState State => this.operationState;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[OperationOrchestration Operation=\"{this.operation.Name}\" State={this.operationState}]";
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (!GameStateProvider.Current.Situations.Any(x => x.SituationId == this.SituationId))
            {
                // Situation was removed, abort.
                Autoccultist.LogWarn($"Situation {this.SituationId} was removed while executing operation {this.operation.Name}.");
                this.Abort();
                return;
            }

            if (this.operationState != OperationState.Unstarted)
            {
                return;
            }

            var situation = this.GetSituationState();

            switch (situation.State)
            {
                case StateEnum.Unstarted:
                case StateEnum.RequiringExecution:
                    this.operationState = OperationState.Starting;
                    BrainEventSink.OnOperationStarted(this.operation);
                    this.RunCoroutine(this.StartOperationCoroutine, nameof(this.StartOperationCoroutine));
                    break;
                case StateEnum.Ongoing:
                    this.operationState = OperationState.Ongoing;
                    BrainEventSink.OnOperationStarted(this.operation);
                    this.ContinueOperation();
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
            if (!GameStateProvider.Current.Situations.Any(x => x.SituationId == this.SituationId))
            {
                // Situation was removed, abort.
                Autoccultist.LogWarn($"Situation {this.SituationId} was removed while executing operation {this.operation.Name}.");
                this.Abort();
                return;
            }

            if (this.currentCoroutine != null)
            {
                // We are waiting on a coroutine to finish.
                return;
            }

            if (this.operationState == OperationState.Ongoing)
            {
                this.UpdateOngoing();
            }
            else if (this.operationState == OperationState.Orphaning)
            {
                this.UpdateOrphaning();
            }
        }

        /// <inheritdoc/>
        public void Abort()
        {
            if (this.cancelCurrentTask != null)
            {
                this.cancelCurrentTask.Cancel();
            }

            this.End(true);
        }

        private ISituationState GetSituationState()
        {
            var situation = GameStateProvider.Current.Situations.FirstOrDefault(x => x.SituationId == this.SituationId);
            if (situation == null)
            {
                throw new ApplicationException($"Tried to run an operation orchistration for situation {this.SituationId}, but no situation was found.");
            }

            return situation;
        }

        private void Complete()
        {
            var situation = this.GetSituationState();

            Autoccultist.LogTrace($"Completing operation {this.operation.Name} on recipe {situation.CurrentRecipe}.");

            // We currently assume mansus only ever occurs as the last recipe of a situation.
            // FIXME: This is quite janky...  We just hope the portal manages to open by the time we notice completion.
            // Hopefully it opens instantaniously before we get to it.
            // We dont even know this is our portal, although we can guess by checking to see if our situation has a portal assigned to it.
            if (situation.CurrentRecipePortal != null && GameStateProvider.Current.Mansus.State != PortalActiveState.Closed)
            {
                var recipeSolution = this.operation.GetRecipeSolution(situation);
                if (recipeSolution != null && recipeSolution.MansusChoice != null)
                {
                    Autoccultist.LogTrace($"Choosing mansus card for operation {this.operation.Name} portal {situation.CurrentRecipePortal}.");
                    this.operationState = OperationState.ChoosingPortalCard;
                    this.RunCoroutine(token => this.ChooseMansusCoroutine(recipeSolution.MansusChoice, token), nameof(this.ChooseMansusCoroutine));
                    return;
                }
                else
                {
                    Autoccultist.LogWarn($"Unhandled mansus event for recipeId {situation.CurrentRecipe} in operation {this.operation.Name}.");
                }
            }

            this.operationState = OperationState.Completing;
            this.RunCoroutine(this.CompleteOperationCoroutine, nameof(this.CompleteOperationCoroutine));
        }

        private void UpdateOngoing()
        {
            var situation = this.GetSituationState();
            var clockState = situation.State;

            if (clockState == StateEnum.Complete || clockState == StateEnum.Unstarted)
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
                    // Enough time has passed that we can consider the operation completed.
                    this.Complete();
                }

                return;
            }

            if (clockState == StateEnum.Ongoing)
            {
                // The situation is running its recipe.
                //  Reset our state in case we were tracking a previous SituationClock.State == Complete.
                this.completionDebounceTime = null;

                // See if we need to slot new cards.
                this.ContinueOperation();
                return;
            }
        }

        private void UpdateOrphaning()
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

            // Recipe changed.
            this.End();
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

            this.ongoingRecipe = currentRecipe;
            this.ongoingRecipeTimeRemaining = situation.RecipeTimeRemaining ?? 0;

            var recipeSolution = this.operation.GetRecipeSolution(situation);

            if (recipeSolution == null)
            {
                // Operation does not know this recipe.
                return;
            }

            this.RunCoroutine(token => this.ContinueSituationCoroutine(recipeSolution, token), nameof(this.ContinueSituationCoroutine));
        }

        private async Task StartOperationCoroutine(CancellationToken cancellationToken)
        {
            var stateOnBegin = this.GetSituationState();
            var recipeSolution = this.operation.GetRecipeSolution(stateOnBegin);

            if (recipeSolution == null)
            {
                throw new OperationFailedException($"Error in operation {this.operation.Name}: No starting recipe defined.");
            }

            await new ExecuteRecipeAction(this.SituationId, recipeSolution, $"{this.operation.Name} => startingRecipe", true).ExecuteAndWait(cancellationToken);

            var stateOnStarted = this.GetSituationState();
            this.ongoingRecipe = stateOnStarted.CurrentRecipe;
            this.ongoingRecipeTimeRemaining = stateOnStarted.RecipeTimeRemaining ?? 0;

            if (recipeSolution.EndOperation)
            {
                await new CloseSituationAction(this.SituationId).ExecuteAndWait(cancellationToken);
                this.operationState = OperationState.Orphaning;
                return;
            }

            // Mark us as ongoing now that we started the recipe.
            this.operationState = OperationState.Ongoing;

            // Accept the current recipe and fill its needs
            var stateOnOngoing = this.GetSituationState();
            var followupRecipeSolution = this.operation.GetRecipeSolution(stateOnOngoing);
            if (followupRecipeSolution != null)
            {
                await new ExecuteRecipeAction(this.SituationId, followupRecipeSolution, $"{this.operation.Name} => {stateOnOngoing.CurrentRecipe}").Execute(cancellationToken);

                // Note: The above might wait between starting the recipe and closing the window, meaning this might be a bit off.
                // Really short recipes might trip this up.
                var stateOnFollowup = this.GetSituationState();
                this.ongoingRecipe = stateOnFollowup.CurrentRecipe;
                this.ongoingRecipeTimeRemaining = stateOnFollowup.RecipeTimeRemaining ?? 0;

                await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
            }
            else
            {
                await new CloseSituationAction(this.SituationId).ExecuteAndWait(cancellationToken);
            }

            if (followupRecipeSolution?.EndOperation == true)
            {
                this.operationState = OperationState.Orphaning;
            }
        }

        private async Task ContinueSituationCoroutine(IRecipeSolution recipe, CancellationToken cancellationToken)
        {
            var situationState = this.GetSituationState();

            var slots = situationState.RecipeSlots;
            if (slots.Count == 0)
            {
                return;
            }

            if (slots.Any(x => x.Card != null))
            {
                throw new OperationFailedException($"Error in operation {this.operation.Name} recipe {situationState.CurrentRecipe}: Cannot continue operation as cards are already slotted.");
            }

            await new ExecuteRecipeAction(this.SituationId, recipe, $"{this.operation.Name} => {situationState.CurrentRecipe}").ExecuteAndWait(cancellationToken);

            if (recipe.EndOperation)
            {
                this.operationState = OperationState.Orphaning;
            }
        }

        private async Task ChooseMansusCoroutine(IMansusSolution solution, CancellationToken cancellationToken)
        {
            await new ChooseMansusCardAction(solution).ExecuteAndWait(cancellationToken);

            // A mansus event always ends the situation.
            this.operationState = OperationState.Completing;

            await this.CompleteOperationCoroutine(cancellationToken);
        }

        private async Task CompleteOperationCoroutine(CancellationToken cancellationToken)
        {
            await new ConcludeSituationAction(this.SituationId).ExecuteAndWait(cancellationToken);
            this.End();
        }

        private SlotCardAction GetSlotActionForRecipeSlotSpec(string specId, IRecipeSolution recipe)
        {
            if (!recipe.SlotSolutions.TryGetValue(specId, out var cardChoice))
            {
                return null;
            }

            return new SlotCardAction(this.SituationId, specId, cardChoice);
        }

        private async void RunCoroutine(Func<CancellationToken, Task> coroutine, string coroutineName)
        {
            await this.AwaitCoroutine(coroutine, coroutineName);
        }

        private async Task AwaitCoroutine(Func<CancellationToken, Task> coroutine, string coroutineName)
        {
            if (this.currentCoroutineName != null)
            {
                throw new ApplicationException($"Operation {this.operation.Name} tried to start a new coroutine {coroutineName} while coroutine {this.currentCoroutineName} is already ongoing.");
            }

            try
            {
                this.currentCoroutineName = coroutineName;
                this.cancelCurrentTask = new CancellationTokenSource();

                this.currentCoroutine = Cerebellum.Coordinate(
                    async (innerToken) =>
                    {
                        // Pausing used to be implemented by AutoccultistActor, but now we are responsible for it.
                        // We may already be paused if we are starting, but we are responsible for tracking and updating our own situation,
                        // so we still need to pause for ongoing coroutines.
                        var pauseToken = GameAPI.Pause();
                        try
                        {
                            await coroutine(innerToken);
                        }
                        catch (TaskCanceledException)
                        {
                            // Do nothing.
                        }
                        catch (Exception ex)
                        {
                            await this.OnErrorCoroutine(ex, innerToken);
                        }
                        finally
                        {
                            pauseToken.Dispose();
                        }
                    }, this.cancelCurrentTask.Token);

                await this.currentCoroutine;
            }
            catch (Exception ex)
            {
                Autoccultist.LogWarn(ex, $"Failed to run operation {this.operation.Name}: {ex.Message}");
                this.Abort();
            }
            finally
            {
                this.currentCoroutine = null;
                this.cancelCurrentTask = null;
                this.currentCoroutineName = null;
            }
        }

        private async Task OnErrorCoroutine(Exception ex, CancellationToken cancellationToken)
        {
            Autoccultist.LogWarn($"Operation {this.operation.Name} failed: {ex.Message}");
            Autoccultist.LogWarn(ex.StackTrace);

            try
            {
                var situation = this.GetSituationState();
                if (situation.State == StateEnum.Unstarted || situation.State == StateEnum.RequiringExecution || situation.State == StateEnum.Complete)
                {
                    await new ConcludeSituationAction(this.SituationId).ExecuteAndWait(cancellationToken);
                }
            }
            catch (Exception ex2)
            {
                Autoccultist.LogWarn($"Failed to clean up situation {this.SituationId} after operation {this.operation.Name} failed: {ex2.Message}");
                Autoccultist.LogWarn(ex2.StackTrace);
            }
            finally
            {
                // This will kill our cancellation token, which is fine as this is the last step.
                this.Abort();
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
