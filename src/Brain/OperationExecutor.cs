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
    public class OperationExecutor
    {
        // We need to wait to see if we are really complete, or just transitioning.
        public static TimeSpan CompleteAwaitTime { get; set; } = TimeSpan.FromSeconds(0.2);

        private Operation operation;

        public event EventHandler Completed;

        public Operation Operation
        {
            get
            {
                return this.operation;
            }
        }

        private SituationController Situation
        {
            get
            {
                var situationId = this.operation.Situation;
                var situation = GameAPI.GetSituation(situationId);
                if (situation == null)
                {
                    AutoccultistPlugin.Instance.LogWarn($"Cannot start solution - Situation {situationId} not found.");
                }
                return situation;
            }
        }

        private bool hasBeenOngoing = false;
        private string ongoingRecipe;
        private DateTime? timeCompleted = null;

        public OperationExecutor(Operation operation)
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
            var situation = this.Situation;
            if (situation == null)
            {
                throw new Exception("Tried to update a situation solution with no situation.");
            }

            // TODO: We want to hook into the completion of the situation rather than scanning it every update.

            var state = this.Situation.SituationClock.State;

            if (state == SituationState.Ongoing)
            {
                timeCompleted = null;
                hasBeenOngoing = true;
                this.ContinueOperation();
            }
            else if (hasBeenOngoing)
            {
                // Only check completion if we have ticked enough to start.
                switch (state)
                {
                    // Complete is when a recipe is completed and holding onto tokens.
                    case SituationState.Complete:
                    // Recipes without output can jump to Unstarted
                    case SituationState.Unstarted:
                        // We need to delay on completion to make sure the situation is entirely done, and not just transitioning.
                        if (timeCompleted == null)
                        {
                            timeCompleted = DateTime.Now;
                        }
                        else if (timeCompleted + CompleteAwaitTime < DateTime.Now)
                        {
                            this.Complete();
                        }
                        break;
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
            var situationId = this.Situation.GetTokenId();

            yield return new SetPausedAction(true);
            yield return new OpenSituationAction(situationId);
            yield return new DumpSituationAction(situationId);

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
            yield return new StartSituationRecipeAction(situationId);

            // Accept the current recipe and fill its needs
            this.ongoingRecipe = this.Situation.SituationClock.RecipeId;
            if (this.operation.OngoingRecipes != null && this.operation.OngoingRecipes.TryGetValue(this.ongoingRecipe, out var ongoingRecipeSolution))
            {
                foreach (var item in this.ContinueSituationCoroutine(ongoingRecipeSolution, false))
                {
                    yield return item;
                }
            }

            yield return new CloseSituationAction(situationId);
            yield return new SetPausedAction(false);
        }

        private IEnumerable<IAutoccultistAction> ContinueSituationCoroutine(RecipeSolution recipe, bool standalone = true)
        {
            var situationId = this.Situation.GetTokenId();

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
                yield return new OpenSituationAction(situationId);
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
                yield return new CloseSituationAction(situationId);
                yield return new SetPausedAction(false);
            }
        }

        private IEnumerable<IAutoccultistAction> CompleteOperationCoroutine()
        {
            var situationId = this.Operation.Situation;
            // TODO: open/dump/close window with actor
            try
            {
                yield return new OpenSituationAction(situationId);
                yield return new DumpSituationAction(situationId);
                yield return new CloseSituationAction(situationId);
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

            var situationId = this.Situation.GetTokenId();

            return new SlotCardAction(situationId, slotId, cardChoice);
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
    }
}