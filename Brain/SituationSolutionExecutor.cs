using System;
using System.Collections.Generic;
using System.Linq;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Autoccultist.Brain.Config;

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
                var situation = GameAPI.GetSituation(this.imperative.Verb);
                if (situation == null)
                {
                    AutoccultistMod.Instance.Warn(string.Format("Cannot start solution - Situation {0} not found.", this.imperative.Verb));
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
            AutoccultistMod.Instance.Trace("Starting imperative " + this.imperative.Name);
            var situation = this.Situation;
            if (situation == null)
            {
                return;
            }

            switch (situation.SituationClock.State)
            {
                case SituationState.Unstarted:
                    situation.situationWindow.DumpAllStartingCardsToDesktop();
                    break;
                case SituationState.Complete:
                    situation.situationWindow.DumpAllResultingCardsToDesktop();
                    break;
                default:
                    AutoccultistMod.Instance.Warn(string.Format("Cannot start solution - Situation {0} in improper state.", this.imperative.Verb));
                    return;
            }

            // TODO: Reserve cards we are going to use, especially those for ongoing slots.
            this.ExecuteRecipeSolution(this.imperative.StartingRecipe, () => situation.situationWindow.GetStartingSlots());

            situation.AttemptActivateRecipe();
            if (situation.SituationClock.State == SituationState.Unstarted)
            {
                AutoccultistMod.Instance.Warn("Cannot start solution - Situation failed to activate");
                this.Abort();
            }

            GameAPI.Heartbeat += this.OnHeartbeat;
        }

        private void OnHeartbeat(object sender, EventArgs e)
        {
            var situation = this.Situation;
            if (situation == null)
            {
                this.Abort();
                return;
            }

            switch (this.Situation.SituationClock.State)
            {
                case SituationState.Ongoing:
                    this.TrySlotOngoing();
                    break;
                case SituationState.Complete:
                case SituationState.Unstarted:
                    this.Complete();
                    break;
            }
        }

        private void TrySlotOngoing()
        {
            var situation = this.Situation;
            if (situation == null)
            {
                return;
            }

            if (this.imperative.OngoingRecipes == null)
            {
                return;
            }

            var recipeId = situation.SituationClock.RecipeId;
            RecipeSolution recipeSolution;
            if (!this.imperative.OngoingRecipes.TryGetValue(recipeId, out recipeSolution))
            {
                return;
            }

            this.ExecuteRecipeSolution(recipeSolution, () => situation.situationWindow.GetOngoingSlots());
        }

        private void ExecuteRecipeSolution(RecipeSolution recipeSolution, Func<IList<RecipeSlot>> slotsRetriever)
        {
            // First slot controls what the rest of the slots are.
            var firstSlot = slotsRetriever().First();
            if (!TryResolveSlot(recipeSolution, firstSlot))
            {
                AutoccultistMod.Instance.Warn("First slot rejected card, skipping remaining slots.");
                return;
            }

            var remainingSlots = slotsRetriever().Skip(1);
            foreach (var slot in remainingSlots)
            {
                TryResolveSlot(recipeSolution, slot);
            }
        }

        private bool TryResolveSlot(RecipeSolution recipeSolution, RecipeSlot slot)
        {
            CardChoice cardChoice;
            if (!recipeSolution.Slots.TryGetValue(slot.GoverningSlotSpecification.Id, out cardChoice))
            {
                return false;
            }

            this.ExecuteSlotSolution(cardChoice, slot);
            return true;
        }

        private void ExecuteSlotSolution(CardChoice slotSolution, RecipeSlot slot)
        {
            // TODO: Choose previously reserved card and clear its reservation.
            // TODO: Move this logic to a card manager and take care of reservations there
            var cards = GameAPI.GetTabletopCards();
            var card = cards.FirstOrDefault(x => slotSolution.CardMatches(x));
            if (card == null)
            {
                return;
            }

            GameAPI.SlotCard(slot, card);
        }


        void Abort()
        {
            this.End();
        }

        void Complete()
        {
            var situation = this.Situation;
            if (situation != null)
            {
                this.Situation.situationWindow.DumpAllResultingCardsToDesktop();
            }
            this.End();
        }

        void End()
        {
            // TODO: Cancel remaining unused reservations
            GameAPI.Heartbeat -= this.OnHeartbeat;
            if (this.Completed != null)
            {
                this.Completed(this, EventArgs.Empty);
            }
        }
    }
}