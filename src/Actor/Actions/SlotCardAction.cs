namespace Autoccultist.Actor.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using Assets.CS.TabletopUI;

    /// <summary>
    /// An action to slot a card into a slot of a situation.
    /// </summary>
    public class SlotCardAction : IAutoccultistAction
    {
        // TODO: This should take a specific card reservation, not a card matcher.

        /// <summary>
        /// Initializes a new instance of the <see cref="SlotCardAction"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to slot the card into.</param>
        /// <param name="slotId">The slot id of the slot in the situation to slot the card into.</param>
        /// <param name="cardMatcher">The card matcher to choose a card to slot.</param>
        public SlotCardAction(string situationId, string slotId, ICardChooser cardMatcher)
        {
            this.SituationId = situationId;
            this.SlotId = slotId;
            this.CardMatcher = cardMatcher;
        }

        /// <summary>
        /// Gets the situation id of the situation to slot the card into.
        /// </summary>
        public string SituationId { get; }

        /// <summary>
        /// Gets the slot id of the slot in the situation to slot the card into.
        /// </summary>
        public string SlotId { get; }

        /// <summary>
        /// Gets the card matcher that will get the card to slot.
        /// </summary>
        public ICardChooser CardMatcher { get; }

        /// <inheritdoc/>
        public void Execute()
        {
            if (GameAPI.IsInMansus)
            {
                throw new ActionFailureException(this, "Cannot interact with situations when in the mansus.");
            }

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
