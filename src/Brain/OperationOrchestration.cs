using System;
using System.Collections.Generic;
using System.Linq;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Autoccultist.Brain.Config;
using Autoccultist.Actor;
using Autoccultist.Actor.Actions;

namespace Autoccultist.Brain
{
    public class OperationOrchestration : ISituationOrchestration
    {
        // We need to wait to see if we are really complete, or just transitioning.
        public static TimeSpan CompleteAwaitTime { get; set; } = TimeSpan.FromSeconds(0.2);

        private Operation operation;

        public event EventHandler Completed;

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

        private OperationState operationState = OperationState.Starting;
        private string ongoingRecipe;
        private DateTime? completionDebounceTime = null;

        public OperationOrchestration(Operation operation)
        {
            this.operation = operation;
        }

        public void Start()
        {
            var situation = this.Situation;
            if (situation == null)
            {
                throw new Exception("Tried to start a situation solution with no situation.");
            }

            AutoccultistPlugin.Instance.LogTrace("Starting operation " + this.operation.Name);
            this.RunCoroutine(this.StartOperationCoroutine());
        }

        public void Update()
        {
            if (this.Situation == null)
            {
                throw new Exception("Tried to update a situation solution with no situation.");
            }

            // TODO: We want to hook into the completion of the situation rather than scanning it every update.

            var state = this.Situation.SituationClock.State;

            if (state == SituationState.Ongoing)
            {
                // The situation is running its recipe.
                //  Reset our state in case we were tracking a previous SituationClock.State == Complete.
                this.operationState = OperationState.Ongoing;
                this.completionDebounceTime = null;

                // See if we need to slot new cards.
                this.ContinueOperation();
            }
            else if (this.operationState == OperationState.Ongoing)
            {
                // At this point, we have seen the situation go through a recipe, so we are clear to start tracking completions.

                // We need to be careful, as this might be a transient state, and the situation may still have another recipe to run.

                // We have two final states: Complete (has output cards) and Unstarted (no output cards).
                if (state == SituationState.Complete || state == SituationState.Unstarted)
                {
                    if (completionDebounceTime == null)
                    {
                        // Start the timer for waiting out to see if this is a real completion.
                        // Situations may tenatively say Complete when they are transitioning to a continuation recipe,
                        //  so we need to delay to make sure this is a full and final completion.
                        completionDebounceTime = DateTime.Now + CompleteAwaitTime;
                    }
                    else if (completionDebounceTime <= DateTime.Now)
                    {
                        // Enough time has passed that we can consider this operation completed.
                        this.operationState = OperationState.Completing;
                        this.Complete();
                    }
                }
            }
        }

        private void Abort()
        {
            AutoccultistPlugin.Instance.LogTrace($"Aborting operation {this.operation.Name}");
            this.End();
        }

        private void Complete()
        {
            AutoccultistPlugin.Instance.LogTrace($"Completing operation {this.operation.Name}");
            this.RunCoroutine(this.CompleteOperationCoroutine());
        }

        private void End()
        {
            if (this.Completed != null)
            {
                this.Completed(this, EventArgs.Empty);
            }
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
            yield return new SetPausedAction(true);
            yield return new OpenSituationAction(this.SituationId);
            yield return new DumpSituationAction(this.SituationId);

            // TODO: Use reserved cards

            // Get the first card.  Slotting this will usually create additional slots
            var slots = this.Situation.situationWindow.GetStartingSlots();
            var firstSlot = slots.First();
            var firstSlotAction = CreateSlotActionFromRecipe(firstSlot, this.operation.StartingRecipe);
            if (firstSlotAction == null)
            {
                // First slot of starting situation is required.
                throw new OperationFailedException($"Error in operation {this.operation.Name}: Slot id {firstSlot.GoverningSlotSpecification.Id} has no card choice.");
            }
            yield return firstSlotAction;

            // Refresh the slots and get the rest of the cards
            slots = this.Situation.situationWindow.GetStartingSlots();
            foreach (var slot in slots.Skip(1))
            {
                var slotAction = CreateSlotActionFromRecipe(slot, this.operation.StartingRecipe);
                if (slotAction != null)
                {
                    yield return slotAction;
                }
            }

            // Start the situation
            yield return new StartSituationRecipeAction(this.SituationId);

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
            yield return new SetPausedAction(false);
        }

        private IEnumerable<IAutoccultistAction> ContinueSituationCoroutine(RecipeSolution recipe, bool standalone = true)
        {
            var slots = this.Situation.situationWindow.GetOngoingSlots();
            if (slots.Count == 0)
            {
                // Nothing to do.
                yield break;
            }

            var firstSlot = slots.First();

            if (firstSlot.GetTokenInSlot() != null)
            {
                // Something is already slotted in, we already handled this.
                yield break;
            }

            if (standalone)
            {
                yield return new SetPausedAction(true);
                yield return new OpenSituationAction(this.SituationId);
            }

            // Get the first card.  Slotting this will usually create additional slots
            var firstSlotAction = CreateSlotActionFromRecipe(firstSlot, recipe);
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
                var slotAction = CreateSlotActionFromRecipe(slot, recipe);
                if (slotAction != null)
                {
                    yield return slotAction;
                }
            }

            if (standalone)
            {
                yield return new CloseSituationAction(this.SituationId);
                yield return new SetPausedAction(false);
            }
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
                this.End();
            }
        }

        private SlotCardAction CreateSlotActionFromRecipe(RecipeSlot slot, RecipeSolution recipe)
        {
            var slotId = slot.GoverningSlotSpecification.Id;
            if (!recipe.Slots.TryGetValue(slotId, out var cardChoice))
            {
                return null;
            }

            return new SlotCardAction(this.SituationId, slotId, cardChoice);
        }

        private async void RunCoroutine(IEnumerable<IAutoccultistAction> coroutine)
        {
            // Note: PerformActions gives us a Task that crawls along on the main thread.
            //  Because of this, Waiting this task will create a deadlock.
            // Async functions are fine, as they are broken up into state machines with Continue
            try
            {
                await AutoccultistActor.PerformActions(coroutine);
            }
            catch (Exception ex)
            {
                AutoccultistPlugin.Instance.LogWarn($"Failed to run operation {this.operation.Name}: {ex.Message}");
                this.Abort();
            }
        }

        enum OperationState
        {
            Starting,
            // The operation is performing its recipes and waiting on the result
            Ongoing,

            // The operation has finished and we are waiting for the contents to dump.
            Completing
        }
    }
}