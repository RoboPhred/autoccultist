using System;
using System.Collections.Generic;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Autoccultist.Brain.Config;

namespace Autoccultist.Brain
{
    public class SituationSolutionExecutor
    {
        private Imperative situationSolution;
        private bool isStarted = false;

        public event EventHandler Completed;

        public Imperative Solution
        {
            get
            {
                return this.situationSolution;
            }
        }

        private SituationController Situation
        {
            get
            {
                var situation = GameAPI.GetSituation(this.situationSolution.SituationID);
                if (situation == null)
                {
                    AutoccultistMod.Instance.Warn(string.Format("Cannot start solution - Situation {0} not found.", this.situationSolution.SituationID));
                }
                return situation;
            }
        }

        public SituationSolutionExecutor(Imperative solution)
        {
            this.situationSolution = solution;
        }

        public void Start()
        {
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
                    AutoccultistMod.Instance.Warn(string.Format("Cannot start solution - Situation {0} in improper state.", this.situationSolution.SituationID));
                    return;
            }

            // TODO: Reserve cards we are going to use, especially those for ongoing slots.

            var startingSlots = situation.situationWindow.GetStartingSlots();
            this.ExecuteRecipeSolution(this.situationSolution.StartingRecipeSolution, startingSlots);

            situation.AttemptActivateRecipe();
            if (situation.SituationClock.State == SituationState.Unstarted)
            {
                AutoccultistMod.Instance.Warn("Cannot start solution - Situation failed to activate");
                this.Abort();
            }

            this.isStarted = true;
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

            var recipeId = situation.SituationClock.RecipeId;
            RecipeSolution recipeSolution;
            if (!this.situationSolution.OngoingRecipeSolutions.TryGetValue(recipeId, out recipeSolution))
            {
                return;
            }

            var ongoingSlots = situation.situationWindow.GetOngoingSlots();
            this.ExecuteRecipeSolution(recipeSolution, ongoingSlots);
        }

        private void ExecuteRecipeSolution(RecipeSolution recipeSolution, IList<RecipeSlot> slots)
        {
            foreach (var slot in slots)
            {
                SlotSolution slotSolution;
                if (!recipeSolution.SlotSolutions.TryGetValue(slot.GoverningSlotSpecification.Id, out slotSolution))
                {
                    continue;
                }

                this.ExecuteSlotSolution(slotSolution, slot);
            }
        }

        private void ExecuteSlotSolution(SlotSolution slotSolution, RecipeSlot slot)
        {
            // TODO: Choose previously reserved card and clear its reservation.
            var card = GameAPI.GetSingleCard(slotSolution.ElementID);
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