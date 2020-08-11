// TODO: Choose previously reserved card and clear its reservation.
// TODO: Move this logic to a card manager and take care of reservations there
using System.Collections.Generic;
using System.Linq;
using Assets.CS.TabletopUI;

namespace Autoccultist.Actor.Actions
{
    class SlotCardAction : IAutoccultistAction
    {
        public string SituationId { get; private set; }
        public string SlotId { get; private set; }
        public ICardMatcher CardMatcher { get; private set; }

        // TODO: This should take a specific card reservation, not a card matcher.
        public SlotCardAction(string situationId, string slotId, ICardMatcher cardMatcher)
        {
            this.SituationId = situationId;
            this.SlotId = slotId;
            this.CardMatcher = cardMatcher;
        }
        public bool CanExecute()
        {
            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                return false;
            }

            switch (situation.SituationClock.State)
            {
                case SituationState.Complete:
                case SituationState.FreshlyStarted:
                    return false;
            }

            IList<RecipeSlot> slots;
            switch (situation.SituationClock.State)
            {
                case SituationState.Unstarted:
                    slots = situation.situationWindow.GetStartingSlots();
                    break;
                case SituationState.Ongoing:
                    slots = situation.situationWindow.GetOngoingSlots();
                    break;
                default:
                    throw new ActionFailureException(this, "Situation is not in an appropriate state to slot cards.");
            }

            var slot = slots.FirstOrDefault(x => x.GoverningSlotSpecification.Id == this.SlotId);
            if (!slot)
            {
                return false;
            }

            var card = CardManager.ChooseCard(this.CardMatcher);
            if (card == null)
            {
                return false;
            }


            return true;
        }

        public void Execute()
        {
            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                throw new ActionFailureException(this, "Situation is not available.");
            }

            IList<RecipeSlot> slots;
            switch (situation.SituationClock.State)
            {
                case SituationState.Unstarted:
                    slots = situation.situationWindow.GetStartingSlots();
                    break;
                case SituationState.Ongoing:
                    slots = situation.situationWindow.GetOngoingSlots();
                    break;
                default:
                    throw new ActionFailureException(this, "Situation is not in an appropriate state to slot cards.");
            }

            var slot = slots.FirstOrDefault(x => x.GoverningSlotSpecification.Id == this.SlotId);
            if (!slot)
            {
                throw new ActionFailureException(this, "Situation has no matching slot.");
            }

            var card = CardManager.ChooseCard(this.CardMatcher);
            if (card == null)
            {
                throw new ActionFailureException(this, "No matching card was found.");
            }

            GameAPI.SlotCard(slot, card);
        }
    }
}