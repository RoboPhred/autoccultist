namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Actor;
    using AutoccultistNS.Actor.Actions;
    using AutoccultistNS.GameResources;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Tasks;
    using SecretHistories.Enums;

    public class OperationReaction : SituationReaction
    {
        private Task loopTask;
        private CancellationTokenSource cancellationSource = new();
        private IRecipeSolution currentRecipeSolution;
        private bool isEnding = false;

        // There is confusion between the current recipe we are solving for and the resulting recipe of the cards we have slotted.
        // This list is for the former.
        private List<OperationHistory> history = new();

        public OperationReaction(IOperation operation)
            : base(operation.Situation)
        {
            this.Operation = operation;
        }

        public IOperation Operation { get; }

        public IReadOnlyList<OperationHistory> History => this.history;

        public override string ToString()
        {
            return $"[{this.Operation.Situation}] {this.Operation.Name}";
        }

        public override void Start()
        {
            if (this.loopTask != null)
            {
                throw new ReactionFailedException($"Cannot start OperationReaction more than once.");
            }

            if (!GameResource.Of<ISituationState>().TryAddConstraint(this))
            {
                throw new ReactionFailedException($"Cannot start operation {this.Operation} as situation resource is already busy.");
            }

            this.loopTask = this.ExecuteOperation();
        }

        public string DebugCurrentRecipe()
        {
            var state = this.GetSituationState();
            var recipeSolution = this.Operation.GetRecipeSolution(state);
            if (recipeSolution == null)
            {
                return "No recipe";
            }

            return recipeSolution.ToString();
        }

        protected override void OnAbort()
        {
            this.End(true);
        }

        private async Task ExecuteOperation()
        {
            BrainEvents.OnOperationStarted(this.Operation);
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
            // Start the operation.
            // This involves either starting the verb, or slotting our first ongoing recipe.
            if (!await this.StartOperation())
            {
                this.End();
                return;
            }

            // Loop through all remaining recipes.
            while (await this.AwaitRecipeChanged())
            {
                if (!await this.TryExecuteCurrentRecipe())
                {
                    this.End();
                    return;
                }
            }

            // Whatever state we ended up in, we are no longer ongoing.
            // Try to complete the operation.
            await this.CompleteOperation();

            this.End();
        }

        private async Task<bool> StartOperation()
        {
            var state = this.GetSituationState();
            if (state.State == StateEnum.Unstarted)
            {
                if (!await this.StartSituation())
                {
                    return false;
                }
            }
            else if (state.State == StateEnum.Ongoing)
            {
                if (!await this.TryExecuteCurrentRecipe())
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> AwaitRecipeChanged()
        {
            var state = this.GetSituationState();
            var currentRecipeRemaining = state.RecipeTimeRemaining ?? 0;

            var populatedSlotCount = state.GetSlottedCards().Count();

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

                    // Note: ongoing recipes are always restricted to a single slot, so there is zero risk of re-slotting slots that have not emptied.
                    // Even so, ExecuteRecipeAction should kick out slotted cards when re-executing.
                    if (this.currentRecipeSolution != null && this.currentRecipeSolution.RerunOnTheft && newState.GetSlottedCards().Count() < populatedSlotCount)
                    {
                        // We lost a card, and we're allowed to re-run the recipe.
                        return true;
                    }

                    // Keep tracking the value as it drops.
                    // We want to trigger once it jumps up again.
                    currentRecipeRemaining = newState.RecipeTimeRemaining.Value;

                    // Recipe is still doing its thing.
                    return false;
                },
                this.cancellationSource.Token);

            // Whatever we were doing is now done.
            this.currentRecipeSolution = null;

            // Return if we have a recipe to handle
            return this.GetSituationState().State == StateEnum.Ongoing;
        }

        private Task<bool> StartSituation()
        {
            return GameAPI.WhilePaused(
                $"{this.GetType().Name}:{nameof(this.StartSituation)}",
                async () =>
                {
                    // Can't do this in Cerebellum or we could block the real mansus handler and deadlock.
                    await GameAPI.AwaitNotInMansus(this.cancellationSource.Token);

                    return await Cerebellum.Coordinate(
                        async (innerToken) =>
                        {
                            var state = this.GetSituationState();
                            var recipeSolution = this.Operation.GetRecipeSolution(state);

                            if (recipeSolution == null)
                            {
                                throw new ReactionFailedException($"Error in operation {this.Operation.Name}: No starting recipe defined.");
                            }

                            this.currentRecipeSolution = recipeSolution;

                            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                            {
                                await new OpenSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                                state = this.GetSituationState();
                                if (state.State == StateEnum.Unstarted)
                                {
                                    await new DumpUnstartedSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                                }
                            }

                            await new ExecuteRecipeAction(this.Operation.Situation, recipeSolution, $"{this.Operation.Name} => startingRecipe").ExecuteAndWait(innerToken);

                            this.CaptureHistory();

                            BrainEvents.OnOperationRecipeExecuted(this.Operation, this.GetSituationState().CurrentRecipe, this.GetSituationState().RecipeSlots.ToDictionary(x => x.SpecId, x => x.Card));

                            await new StartSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);

                            state = this.GetSituationState();

                            if (recipeSolution.EndOperation)
                            {
                                if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                                {
                                    await new CloseSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                                }

                                return false;
                            }

                            // Get and satisfy the first ongoing situation.
                            recipeSolution = this.Operation.GetRecipeSolution(state);
                            if (recipeSolution == null)
                            {
                                // Don't know what this recipe is, let it continue.
                                if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                                {
                                    await new CloseSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                                }

                                return true;
                            }

                            this.currentRecipeSolution = recipeSolution;

                            await new ExecuteRecipeAction(this.Operation.Situation, recipeSolution, $"{this.Operation.Name} => ongoingRecipe").ExecuteAndWait(innerToken);

                            BrainEvents.OnOperationRecipeExecuted(this.Operation, this.GetSituationState().CurrentRecipe, this.GetSituationState().RecipeSlots.ToDictionary(x => x.SpecId, x => x.Card));

                            this.CaptureHistory();

                            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                            {
                                await new CloseSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            }

                            return !recipeSolution.EndOperation;
                        },
                        this.cancellationSource.Token,
                        $"StartOperation {this.Operation.Name}");
                },
                this.cancellationSource.Token);
        }

        private async Task<bool> TryExecuteCurrentRecipe()
        {
            // Do this ahead of time so we know not to pause if there is nothing to do.
            var state = this.GetSituationState();

            // Remember what this was, in case it somehow ends up changing.
            var pinnedRecipe = state.CurrentRecipe;

            // FIXME: If there are magnet slots, wait for some time until they grab something.
            // Ideally, we would wait ALL the time, so we can record that they never get what they are looking for
            var recipeSolution = this.Operation.GetRecipeSolution(state);
            if (recipeSolution == null)
            {
                // Don't know what this recipe is, let it continue.
                this.CaptureHistory();
                return true;
            }

            if (state.RecipeSlots.Count == 0)
            {
                // Nothing to slot, no need to coordinate anything.
                this.CaptureHistory();
                return !recipeSolution.EndOperation;
            }

            return await GameAPI.WhilePaused(
                $"{this.GetType().Name}:{nameof(this.TryExecuteCurrentRecipe)}",
                async () =>
                {
                    // Can't do this in Cerebellum or we could deadlock.
                    await GameAPI.AwaitNotInMansus(this.cancellationSource.Token);

                    return await Cerebellum.Coordinate(
                        async (innerToken) =>
                        {
                            // Re-check just in case we managed to change before it was our turn to run.
                            var state = this.GetSituationState();

                            if (state.CurrentRecipe != pinnedRecipe)
                            {
                                Autoccultist.LogWarn($"Situation {state.SituationId} recipe changed while TryExecuteCurrentRecipe was waiting for our turn to run: {pinnedRecipe} => {state.CurrentRecipe}");
                            }

                            var recipeSolution = this.Operation.GetRecipeSolution(state);
                            if (recipeSolution == null)
                            {
                                // This can happen either because the current recipe changed, or because the game state changed and a conditional recipe no longer applies.
                                Autoccultist.LogWarn($"TryExecuteCurrentRecipe had a recipe for {pinnedRecipe}, but GetRecipeSolution can no longer find it.");

                                // Don't know what this recipe is, let it continue.
                                return true;
                            }

                            this.currentRecipeSolution = recipeSolution;

                            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                            {
                                await new OpenSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            }

                            await new ExecuteRecipeAction(this.Operation.Situation, recipeSolution, $"{this.Operation.Name} => ongoingRecipe").ExecuteAndWait(innerToken);

                            this.CaptureHistory();

                            BrainEvents.OnOperationRecipeExecuted(this.Operation, this.GetSituationState().CurrentRecipe, this.GetSituationState().RecipeSlots.ToDictionary(x => x.SpecId, x => x.Card));

                            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                            {
                                await new CloseSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            }

                            return !recipeSolution.EndOperation;
                        },
                        this.cancellationSource.Token,
                        $"TryExecuteCurrentRecipe {this.Operation.Name}");
                },
                this.cancellationSource.Token);
        }

        private async Task CompleteOperation()
        {
            await GameAPI.WhilePaused(
                $"{this.GetType().Name}:{nameof(this.CompleteOperation)}",
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
                        async (innerToken) =>
                        {
                            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                            {
                                await new OpenSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                                await new RevealTokensSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            }

                            BrainEvents.OnOperationCompleted(this.Operation, this.GetSituationState().OutputCards);

                            await new ConcludeSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                            {
                                await new CloseSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            }
                        },
                        this.cancellationSource.Token);
                },
                this.cancellationSource.Token);
        }

        private void CaptureHistory()
        {
            var state = this.GetSituationState();

            var cards = state.GetSlottedCards();
            var history = new OperationHistory(state.CurrentRecipe, state.SlottedRecipe, cards.Select(x => x.ElementId).ToArray());

            this.history.Add(history);
        }

        private async Task HandleOperationError(Exception ex)
        {
            Autoccultist.LogWarn(ex, $"Operation {this.Operation.Name} failed: {ex.Message}");

            var pause = GameAPI.Pause("HandleOperationError");
            try
            {
                var situation = this.GetSituationState();
                if (situation.State == StateEnum.Unstarted || situation.State == StateEnum.RequiringExecution || situation.State == StateEnum.Complete)
                {
                    await Cerebellum.Coordinate(
                        async (innerToken) =>
                        {
                            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                            {
                                await new OpenSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            }

                            if (situation.State == StateEnum.Complete)
                            {
                                await new ConcludeSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            }
                            else
                            {
                                await new DumpUnstartedSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            }

                            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                            {
                                await new CloseSituationAction(this.Operation.Situation).ExecuteAndWait(innerToken);
                            }
                        },
                        this.cancellationSource.Token);
                }
            }
            catch (Exception ex2)
            {
                Autoccultist.LogWarn(ex2, $"Failed to clean up situation {this.Operation.Situation} after operation {this.Operation.Name} failed: {ex2.Message}");
            }
            finally
            {
                pause.Dispose();
                this.Abort();
            }
        }

        private async void End(bool aborted = false)
        {
            if (this.IsCompleted)
            {
                return;
            }

            // Gate End behind its own flag, as cancelling the cancellationSource may cause End to be called again.
            if (this.isEnding)
            {
                return;
            }

            this.isEnding = true;

            this.cancellationSource.Cancel();

            if (this.loopTask != null)
            {
                try
                {
                    await this.loopTask;
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Autoccultist.LogWarn(ex, $"Exception while waiting for op ${this.Operation.Name} to end.");
                }
            }

            if (aborted)
            {
                BrainEvents.OnOperationAborted(this.Operation);
            }

            BrainEvents.OnOperationEnded(this.Operation);

            this.TryComplete(aborted);
        }

        public class OperationHistory
        {
            public OperationHistory(string recipeId, string slottedRecipeId, IReadOnlyCollection<string> slottedElements)
            {
                this.RecipeId = recipeId;
                this.SlottedRecipeId = slottedRecipeId;
                this.SlottedElements = slottedElements;
            }

            public string RecipeId { get; private set; }

            public string SlottedRecipeId { get; private set; }

            public IReadOnlyCollection<string> SlottedElements { get; private set; }
        }
    }
}
