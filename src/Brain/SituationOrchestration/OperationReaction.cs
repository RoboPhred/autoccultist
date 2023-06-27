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
    using AutoccultistNS.Resources;
    using AutoccultistNS.Tasks;
    using SecretHistories.Enums;

    public class OperationReaction : IReaction, IResourceConstraint<ISituationState>
    {
        private bool isDisposed;

        private Task loopTask;
        private CancellationTokenSource cancellationSource = new();

        public OperationReaction(IOperation operation)
        {
            this.Operation = operation;
        }

        /// <inheritdoc/>
        public event EventHandler Completed;

        /// <inheritdoc/>
        public event EventHandler Disposed;

        public IOperation Operation { get; }

        IEnumerable<ISituationState> IResourceConstraint<ISituationState>.GetCandidates()
        {
            return new[] { this.GetSituationState() };
        }

        public override string ToString()
        {
            return $"OperationReaction({this.Operation})";
        }

        public void Abort()
        {
            this.End(true);
        }

        public void Start()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(OperationReaction));
            }

            if (this.loopTask != null)
            {
                throw new ReactionFailedException($"Cannot start OperationReaction more than once.");
            }

            if (!Resource.Of<ISituationState>().TryAddConstraint(this))
            {
                throw new ReactionFailedException($"Cannot start operation {this.Operation} as situation resource is already busy.");
            }

            this.loopTask = this.ExecuteOperation();
        }

        public async void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            if (!this.cancellationSource.IsCancellationRequested)
            {
                this.cancellationSource.Cancel();
            }

            try
            {
                await this.loopTask;
            }
            catch (Exception ex)
            {
                Autoccultist.LogWarn(ex, $"Exception while waiting for op ${this.Operation.Name} to dispose.");
            }

            // The fact that we have two of these is a quirk of us implementing both IReaction and IResourceConstraint.
            this.Completed?.Invoke(this, EventArgs.Empty);
            this.Disposed?.Invoke(this, EventArgs.Empty);
        }

        private async Task ExecuteOperation()
        {
            BrainEventSink.OnOperationStarted(this.Operation);
            try
            {
                await this.OperationExecutionLoop();
            }
            catch (TaskCanceledException)
            {
                this.End(true);
            }
            catch (Exception ex)
            {
                await this.HandleOperationError(ex);
            }
        }

        private async Task OperationExecutionLoop()
        {
            bool shouldContinue;
            ISituationState state;

            state = this.GetSituationState();
            if (state.State == StateEnum.Unstarted)
            {
                shouldContinue = await this.StartOperation();
                if (!shouldContinue)
                {
                    this.End();
                    return;
                }
            }

            state = this.GetSituationState();

            // Loop through all the ongoing recipes.
            while (true)
            {
                var currentRecipeRemaining = state.RecipeTimeRemaining ?? 0;

                // Wait for the recipe to change, or for the situation to end.
                // Note: Do we want to re-check if someone unslots our cards?
                // Greedy slots can steal from us...
                await AwaitConditionTask.From(
                    () =>
                    {
                        var newState = this.GetSituationState();
                        if (newState.State != StateEnum.Ongoing)
                        {
                            // No longer ongoing, we must have finished.
                            return true;
                        }

                        if (!newState.RecipeTimeRemaining.HasValue || newState.RecipeTimeRemaining > currentRecipeRemaining)
                        {
                            // The time remaining went up, we must have started a new recipe.
                            return true;
                        }

                        // Keep tracking the value as it drops.
                        // We want to trigger once it jumps up again.
                        currentRecipeRemaining = newState.RecipeTimeRemaining.Value;

                        // Recipe is still doing its thing.
                        return false;
                    },
                    this.cancellationSource.Token);

                // Re-fetch the state now that we waited some time.
                state = this.GetSituationState();
                if (state.State != StateEnum.Ongoing)
                {
                    // All done with recipes, move on to completion.
                    break;
                }

                shouldContinue = await this.TryExecuteCurrentRecipe();

                if (!shouldContinue)
                {
                    this.End();
                    return;
                }
            }

            // Whatever we are, we are no longer ongoing.
            await this.CompleteOperation();

            this.End();
        }

        private async Task<bool> StartOperation()
        {
            // Can't do this in Cerebellum or we could block the real mansus handler and deadlock.
            await GameAPI.AwaitNotInMansus(this.cancellationSource.Token);

            return await GameAPI.WhilePaused(
                () => Cerebellum.Coordinate(
                    async (innerToken) =>
                    {
                        var state = this.GetSituationState();
                        var recipeSolution = this.Operation.GetRecipeSolution(state);

                        if (recipeSolution == null)
                        {
                            throw new OperationFailedException($"Error in operation {this.Operation.Name}: No starting recipe defined.");
                        }

                        await new ExecuteRecipeAction(this.Operation.Situation, recipeSolution, $"{this.Operation.Name} => startingRecipe", true).ExecuteAndWait(innerToken);

                        if (recipeSolution.EndOperation)
                        {
                            await new CloseSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            return false;
                        }

                        // Get and satisfy the first ongoing situation.
                        state = this.GetSituationState();
                        recipeSolution = this.Operation.GetRecipeSolution(state);
                        if (recipeSolution == null)
                        {
                            // Don't know what this recipe is, let it continue.
                            await new CloseSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            return true;
                        }

                        await new ExecuteRecipeAction(this.Operation.Situation, recipeSolution, $"{this.Operation.Name} => ongoingRecipe", true).ExecuteAndWait(innerToken);

                        await new CloseSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);

                        return !recipeSolution.EndOperation;
                    }),
                this.cancellationSource.Token);
        }

        private Task<bool> TryExecuteCurrentRecipe()
        {
            return GameAPI.WhilePaused(
                async () =>
                {
                    // Can't do this in Cerebellum or we could deadlock.
                    await GameAPI.AwaitNotInMansus(this.cancellationSource.Token);

                    return await Cerebellum.Coordinate(
                        async (innerToken) =>
                        {
                            var state = this.GetSituationState();
                            var recipeSolution = this.Operation.GetRecipeSolution(state);
                            if (recipeSolution == null)
                            {
                                // Don't know what this recipe is, let it continue.
                                return true;
                            }

                            await new ExecuteRecipeAction(this.Operation.Situation, recipeSolution, $"{this.Operation.Name} => ongoingRecipe").ExecuteAndWait(innerToken);

                            return !recipeSolution.EndOperation;
                        });
                },
                this.cancellationSource.Token);
        }

        private async Task CompleteOperation()
        {
            await GameAPI.WhilePaused(
                async () =>
                {
                    await Cerebellum.Coordinate(
                        async (innerToken) =>
                        {
                            var state = this.GetSituationState();
                            if (state.CurrentRecipePortal != null && GameStateProvider.Current.Mansus.State != PortalActiveState.Closed)
                            {
                                var recipeSolution = this.Operation.GetRecipeSolution(state);
                                if (recipeSolution != null && recipeSolution.MansusChoice != null)
                                {
                                    await new ChooseMansusCardAction(recipeSolution.MansusChoice).Execute(innerToken);
                                }
                                else
                                {
                                    throw new ReactionFailedException($"Cannot complete operation {this.Operation} as the completing recipe is a portal.");
                                }
                            }
                        }, this.cancellationSource.Token);

                    // Await the mansus outside of any cerebellum coordination, in case we are not the mansus handler.
                    await GameAPI.AwaitNotInMansus(this.cancellationSource.Token);

                    await Cerebellum.Coordinate(
                        (innerToken) => new ConcludeSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken),
                        this.cancellationSource.Token);
                },
                this.cancellationSource.Token);
        }

        private async Task HandleOperationError(Exception ex)
        {
            Autoccultist.LogWarn($"Operation {this.Operation.Name} failed: {ex.Message}");
            Autoccultist.LogWarn(ex.StackTrace);

            var pause = GameAPI.Pause();
            try
            {
                var situation = this.GetSituationState();
                if (situation.State == StateEnum.Unstarted || situation.State == StateEnum.RequiringExecution || situation.State == StateEnum.Complete)
                {
                    await Cerebellum.Coordinate(
                        (innerToken) => new ConcludeSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken),
                        this.cancellationSource.Token);
                }
            }
            catch (Exception ex2)
            {
                Autoccultist.LogWarn($"Failed to clean up situation {this.Operation.Situation} after operation {this.Operation.Name} failed: {ex2.Message}");
                Autoccultist.LogWarn(ex2.StackTrace);
            }
            finally
            {
                pause.Dispose();

                // This will kill our cancellation token, which is fine as this is the last step.
                this.Abort();
            }
        }

        private void End(bool aborted = false)
        {
            this.Dispose();

            if (aborted)
            {
                BrainEventSink.OnOperationAborted(this.Operation);
            }
            else
            {
                BrainEventSink.OnOperationCompleted(this.Operation);
            }
        }

        private ISituationState GetSituationState()
        {
            var state = GameStateProvider.Current.Situations.FirstOrDefault(x => x.SituationId == this.Operation.Situation);
            if (state == null)
            {
                throw new ReactionFailedException($"Situation {this.Operation.Situation} does not exist.");
            }

            return state;
        }
    }
}
