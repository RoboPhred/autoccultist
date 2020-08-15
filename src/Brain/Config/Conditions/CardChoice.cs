namespace Autoccultist.Brain.Config.Conditions
{
    using System.Collections.Generic;
    using System.Linq;
    using Autoccultist;
    using Autoccultist.GameState;

    /// <summary>
    /// Represents a choice of a card based on various attributes.
    /// </summary>
    public class CardChoice : ICardChooser, ICardConditionConfig
    {
        /// <summary>
        /// Gets or sets the element id of the card to choose.
        /// If left empty, the element id will not be factored into the card choice.
        /// </summary>
        public string ElementId { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of aspect names to degrees to filter the cards by.
        /// If set, a matching card must have all of the specified aspects of at least the given degree.
        /// </summary>
        public Dictionary<string, int> Aspects { get; set; }

        /// <summary>
        /// Gets or sets a list of aspects forbidden to be on the chosen card.
        /// Mainly used when specifying matching aspects.
        /// </summary>
        public List<string> ForbiddenAspects { get; set; }

        /// <summary>
        /// Gets or sets a list of elements forbidden from being matched.
        /// Mainly used when specifying matching aspects.
        /// </summary>
        public List<string> ForbiddenElementIds { get; set; }

        /// <inheritdoc/>
        public void Validate()
        {
            if (string.IsNullOrEmpty(this.ElementId) && (this.Aspects == null || this.Aspects.Count == 0))
            {
                throw new InvalidConfigException("Card choice must have either an elementId or aspects.");
            }
        }

        /// <inheritdoc/>
        public ICardState ChooseCard(IEnumerable<ICardState> cards)
        {
            // TODO: We could have some weighing mechanism to let a config specify which cards are higher priority than others?

            // TODO: We also want to filter by time remaining, and preference either least or most time remaining.
            var candidates =
                from card in cards
                where this.ElementId == null || card.ElementId == this.ElementId
                where this.ForbiddenElementIds?.Contains(card.ElementId) != true
                where this.Aspects == null || card.Aspects.HasAspects(this.Aspects)
                where this.ForbiddenAspects?.Intersect(card.Aspects.Keys).Any() != false
                let cardWeight = card.Aspects.GetWeight() - (this.Aspects?.GetWeight() ?? 0)
                orderby cardWeight ascending // We want the lowest weight card we can find
                select card;

            return candidates.FirstOrDefault();
        }

        /// <inheritdoc/>
        public bool IsConditionMet(IGameState state)
        {
            return this.ChooseCard(state.TabletopCards) != null;
        }

        /// <inheritdoc/>
        public bool CardsMatchSet(IReadOnlyCollection<ICardState> cards)
        {
            return this.ChooseCard(cards) != null;
        }
    }
}
