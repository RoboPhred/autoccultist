namespace AutoccultistNS.Actor.Actions
{
    using System.Linq;
    using AutoccultistNS.GameState;
    using SecretHistories.Enums;

    /// <summary>
    /// An action to slot a card into a slot of a situation.
    /// </summary>
    public class SlotCardAction : SyncActionBase
    {
        // TODO: This should take a specific card reservation, not a card matcher.

        /// <summary>
        /// Initializes a new instance of the <see cref="SlotCardAction"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to slot the card into.</param>
        /// <param name="slotId">The slot id of the slot in the situation to slot the card into.</param>
        /// <param name="cardMatcher">The card matcher to choose a card to slot.</param>
        public SlotCardAction(string situationId, string slotId, ISlotCardChooser cardMatcher)
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
        public ISlotCardChooser CardMatcher { get; }

        public override string ToString()
        {
            return $"SlotCardAction(SituationId = {this.SituationId}, SlotId = {this.SlotId})";
        }

        /// <inheritdoc/>
        protected override ActionResult OnExecute()
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

            var card = CardManager.ChooseCard(this.CardMatcher);
            if (card == null)
            {
                if (this.CardMatcher.Optional)
                {
                    return ActionResult.NoOp;
                }

                throw new ActionFailureException(this, $"No matching card was found for situation {this.SituationId} slot {this.SlotId}.");
            }

            var sphere = situation.GetSpheresByCategory(SphereCategory.Threshold).FirstOrDefault(x => x.GoverningSphereSpec.Id == this.SlotId);
            if (sphere == null)
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} has no matching slot {this.SlotId}.");
            }

            var stack = card.ToElementStack();

            // The game sets the home location when we pick up a card or it starts to move, not when we drop it!
            // Without this, the card will jump back to the position it was before the player moved it.
            stack.Token.RequestHomeLocationFromCurrentSphere();

            if (!GameAPI.TrySlotCard(sphere, stack))
            {
                throw new ActionFailureException(this, $"Card was not accepted by the slot {this.SlotId} in situation {this.SituationId}.");
            }

            GameStateProvider.Invalidate();
            return ActionResult.Completed;
        }
    }
}
