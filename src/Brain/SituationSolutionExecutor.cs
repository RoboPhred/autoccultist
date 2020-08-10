using System;
using System.Collections.Generic;
using System.Linq;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Autoccultist.Brain.Config;
using Autoccultist.Hand;
using Autoccultist.Hand.Actions;

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
            if (this.imperative.OngoingRecipes == null)
            {
                // No recipes to continue
                return;
            }

            var slots = this.Situation.situationWindow.GetOngoingSlots();
            if (slots.Count == 0)
            {
                // Nothing to do.
                return;
            }

            var firstSlot = slots.First();
            if (firstSlot.GetTokenInSlot() != null)
            {
                // Already filled
                return;
            }

            var recipeId = Situation.SituationClock.RecipeId;
            if (!this.imperative.OngoingRecipes.TryGetValue(recipeId, out var recipeSolution))
            {
                // Imperative does not know this recipe.
                return;
            }

            this.RunCoroutine(this.ContinueSituationCoroutine(recipeSolution));
        }

        private IEnumerable<IAutoccultistAction> StartSituationCoroutine()
        {
            var situationId = this.Situation.GetTokenId();

            yield return new OpenSituationAction(situationId);
            yield return new DumpSituationAction(situationId);

            // TODO: Use reserved cards

            // Get the first card.  Slotting this will usually create additional slots
            var slots = this.Situation.situationWindow.GetStartingSlots();
            var firstSlot = slots.First();
            yield return CreateSlotActionFromRecipe(firstSlot, this.imperative.StartingRecipe);

            // Refresh the slots and get the rest of the cards
            slots = this.Situation.situationWindow.GetStartingSlots();
            foreach (var slot in slots.Skip(1))
            {
                yield return CreateSlotActionFromRecipe(slot, this.imperative.StartingRecipe);
            }

            // Start the situation
            yield return new StartSituationRecipeAction(situationId);
            yield return new CloseSituationAction(situationId);
        }

        private IEnumerable<IAutoccultistAction> ContinueSituationCoroutine(RecipeSolution recipe)
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

            // Get the first card.  Slotting this will usually create additional slots
            yield return CreateSlotActionFromRecipe(firstSlot, recipe);

            // Refresh the slots and get the rest of the cards
            slots = this.Situation.situationWindow.GetOngoingSlots();
            foreach (var slot in slots.Skip(1))
            {
                yield return CreateSlotActionFromRecipe(slot, recipe);
            }
        }

        private SlotCardAction CreateSlotActionFromRecipe(RecipeSlot slot, RecipeSolution recipe)
        {
            var slotId = slot.GoverningSlotSpecification.Id;
            if (!recipe.Slots.TryGetValue(slotId, out var firstCardChoice))
            {
                throw new RecipeRejectedException($"Error in imperative {this.imperative.Name}: Slot id {slotId} has no card choice.");
            }

            var situationId = this.Situation.GetTokenId();

            return new SlotCardAction(situationId, slotId, firstCardChoice);
        }

        private async void RunCoroutine(IEnumerable<IAutoccultistAction> coroutine)
        {
            AutoccultistPlugin.Instance.LogTrace("Running coroutine");
            // Note: PerformActions gives us a Task that crawls along on the main thread.
            //  Because of this, Waiting this task will create a deadlock.
            // Async functions are fine, as they are broken up into state machines with Continue
            try
            {
                await AutoccultistHand.PerformActions(coroutine);
            }
            catch (Exception ex)
            {
                AutoccultistPlugin.Instance.LogWarn($"Failed to run imperative {this.imperative.Name}: {ex.Message}");
                this.Abort();
            }
            AutoccultistPlugin.Instance.LogTrace("Done Running coroutine");
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