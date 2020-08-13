using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Autoccultist.GameState;

namespace Autoccultist.Actor.Actions
{
    class SlotCardAction : IAutoccultistAction
    {
        public string SituationId { get; private set; }
        public string SlotId { get; private set; }
        public IConsumedToken Card { get; private set; }

        public SlotCardAction(string situationId, string slotId, IConsumedToken card)
        {
            // TODO: Ensure we have a card token.

            this.SituationId = situationId;
            this.SlotId = slotId;
            this.Card = card;
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

            // This may throw if something happened to the card before we got to it.
            var card = this.Card.GetToken() as IElementStack;

            // TODO: Throw if card is already being used by a situation.

            GameAPI.SlotCard(slot, card);

            // Release our lock on the card, now that we made use of it
            this.Card.Release();
        }
    }
}