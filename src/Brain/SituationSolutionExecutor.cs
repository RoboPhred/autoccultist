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
    public class SituationSolutionExecutor
    {
        private Imperative imperative;

        public event EventHandler Completed;

        public Imperative Solution
        {
            get
            {
                return this.imperative;
            }
        }

        private SituationController Situation
        {
            get
            {
                var situation = GameAPI.GetSituation(this.imperative.Situation);
                if (situation == null)
                {
                    AutoccultistPlugin.Instance.LogWarn(string.Format("Cannot start solution - Situation {0} not found.", this.imperative.Situation));
                }
                return situation;
            }
        }

        private string ongoingRecipe;

        public SituationSolutionExecutor(Imperative imperative)
        {
            this.imperative = imperative;
        }

        public void Start()
        {
            var situation = this.Situation;
            if (situation == null)
            {
                throw new Exception("Tried to start a situation solution with no situation.");
            }

            AutoccultistPlugin.Instance.LogTrace("Starting imperative " + this.imperative.Name);
            this.RunCoroutine(this.StartSituationCoroutine());
        }

        public void Update()
        {
            var situation = this.Situation;
            if (situation == null)
            {
                throw new Exception("Tried to update a situation solution with no situation.");
            }

            // TODO: We want to hook into the completion of the situation rather than scanning it every update.

            switch (this.Situation.SituationClock.State)
            {
                case SituationState.Ongoing:
                    this.ContinueSituation();
                    break;
                case SituationState.Complete:
                    this.Complete();
                    break;
            }
        }

        private void ContinueSituation()
        {
            var currentRecipe = this.Situation.SituationClock.RecipeId;
            if (this.ongoingRecipe == currentRecipe)
            {
                // Recipe has not changed.
                return;
            }
            this.ongoingRecipe = currentRecipe;

            if (this.imperative.OngoingRecipes == null)
            {
                // No recipes to continue
                return;
            }

            if (!this.imperative.OngoingRecipes.TryGetValue(currentRecipe, out var recipeSolution))
            {
                // Imperative does not know this recipe.
                return;
            }

            this.RunCoroutine(this.ContinueSituationCoroutine(recipeSolution));
        }

        private IEnumerable<IAutoccultistAction> StartSituationCoroutine()
        {
            var situationId = this.Situation.GetTokenId();

            yield return new SetPausedAction(true);
            yield return new OpenSituationAction(situationId);
            yield return new DumpSituationAction(situationId);

            // TODO: Use reserved cards

            // Get the first card.  Slotting this will usually create additional slots
            var slots = this.Situation.situationWindow.GetStartingSlots();
            var firstSlot = slots.First();
            var firstSlotAction = CreateSlotActionFromRecipe(firstSlot, this.imperative.StartingRecipe);
            if (firstSlotAction == null)
            {
                // First slot of starting situation is required.
                throw new RecipeRejectedException($"Error in imperative {this.imperative.Name}: Slot id {firstSlot.GoverningSlotSpecification.Id} has no card choice.");
            }
            yield return firstSlotAction;

            // Refresh the slots and get the rest of the cards
            slots = this.Situation.situationWindow.GetStartingSlots();
            if (situationId == "dream")
            {
                AutoccultistPlugin.Instance.LogTrace($"Dream has {slots.Count} slots.");
                foreach (var slot in slots)
                {
                    AutoccultistPlugin.Instance.LogTrace($"-- {slot.GoverningSlotSpecification.Id}.");
                }
            }

            foreach (var slot in slots.Skip(1))
            {
                var slotAction = CreateSlotActionFromRecipe(slot, this.imperative.StartingRecipe);
                if (slotAction != null)
                {
                    yield return slotAction;
                }
            }

            // Start the situation
            yield return new StartSituationRecipeAction(situationId);

            // Accept the current recipe and fill its needs
            this.ongoingRecipe = this.Situation.SituationClock.RecipeId;
            if (this.imperative.OngoingRecipes != null && this.imperative.OngoingRecipes.TryGetValue(this.ongoingRecipe, out var ongoingRecipeSolution))
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
                throw new RecipeRejectedException($"Error in imperative {this.imperative.Name}: Slot id {firstSlot.GoverningSlotSpecification.Id} has no card choice.");
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
                AutoccultistPlugin.Instance.LogWarn($"Failed to run imperative {this.imperative.Name}: {ex.Message}");
                this.Abort();
            }
        }

        void Abort()
        {
            this.End();
        }

        void Complete()
        {
            this.Situation.situationWindow.DumpAllResultingCardsToDesktop();
            this.End();
        }

        void End()
        {
            AutoccultistPlugin.Instance.LogTrace($"Ending imperative {this.imperative.Name}");
            if (this.Completed != null)
            {
                this.Completed(this, EventArgs.Empty);
            }
        }
    }
}