namespace AutoccultistNS.Actor.Actions
{
    using System.Linq;
    using AutoccultistNS.GameState;
    using SecretHistories.Enums;

    /// <summary>
    /// An action to slot a card into a slot of a situation.
    /// </summary>
    public class SlotCardAction : IAutoccultistAction
    {
        private bool executed = false;

        // TODO: This should take a specific card reservation, not a card matcher.

        /// <summary>
        /// Initializes a new instance of the <see cref="SlotCardAction"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to slot the card into.</param>
        /// <param name="slotId">The slot id of the slot in the situation to slot the card into.</param>
        /// <param name="cardMatcher">The card matcher to choose a card to slot.</param>
        public SlotCardAction(string situationId, string slotId, ICardChooser cardMatcher)
        {
            NoonUtility.LogWarning("Created slot card action with situation id: " + situationId + " and slot id: " + slotId);
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
            if (this.executed)
            {
                throw new ActionFailureException(this, "This action has already been executed.");
            }

            this.executed = true;

            NoonUtility.LogWarning("Executing slot card action with situation id: " + this.SituationId + " and slot id: " + this.SlotId);

            if (GameAPI.IsInMansus)
            {
                throw new ActionFailureException(this, "Cannot interact with situations when in the mansus.");
            }

            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                throw new ActionFailureException(this, "Situation is not available.");
            }

            var sphere = situation.GetSpheresByCategory(SphereCategory.Threshold).FirstOrDefault(x => x.GoverningSphereSpec.Id == this.SlotId);
            if (sphere == null)
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} has no matching slot {this.SlotId}.");
            }

            var card = CardManager.ChooseCard(this.CardMatcher);
            if (card == null)
            {
                throw new ActionFailureException(this, $"No matching card was found for situation {this.SituationId} slot {this.SlotId}.");
            }

            if (!GameAPI.TrySlotCard(sphere, card.ToElementStack()))
            {
                throw new ActionFailureException(this, $"Card was not accepted by the slot {this.SlotId} in situation {this.SituationId}.");
            }

            GameStateProvider.Invalidate();
            Autoccultist.Instance.LogTrace($"Slotted card {card.ElementId} into situation {this.SituationId} slot {this.SlotId}.");
        }
    }
}
