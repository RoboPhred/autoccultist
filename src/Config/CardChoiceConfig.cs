namespace Autoccultist.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using Autoccultist;
    using Autoccultist.Config.Conditions;
    using Autoccultist.GameState;

    /// <summary>
    /// Represents a choice of a card based on various attributes.
    /// </summary>
    public class CardChoiceConfig : ICardChooser, IConfigObject
    {
        /// <summary>
        /// Specify whether the card choice should go for the oldest or youngest card it can find.
        /// </summary>
        public enum CardAgeSelection
        {
            /// <summary>
            /// The choice should choose the card with the least lifetime remaining.
            /// </summary>
            Oldest,

            /// <summary>
            /// The choice should choose the card with the most lifetime remaining.
            /// </summary>
            Youngest,
        }

        /// <summary>
        /// Gets or sets the element id of the card to choose.
        /// If left empty, the element id will not be factored into the card choice.
        /// </summary>
        public string ElementId { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of aspect names to degrees to filter the cards by.
        /// If set, a matching card must have all of the specified aspects of at least the given degree.
        /// </summary>
        public Dictionary<string, ValueCondition> Aspects { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the card must or must not be unique.
        /// </summary>
        public bool? IsUnique { get; set; }

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

        /// <summary>
        /// Gets or sets the age bias by which to choose time-limited cards.
        /// </summary>
        public CardAgeSelection? AgeBias { get; set; }

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

            // Once again, the lack of covariance in IReadOnlyDictionary comes back to bite us
            var aspectsAsCondition = this.Aspects?.ToDictionary(entry => entry.Key, entry => entry.Value as IValueCondition);

            var candidates =
                from card in cards
                where this.ElementId == null || card.ElementId == this.ElementId
                where this.ForbiddenElementIds?.Contains(card.ElementId) != true
                where aspectsAsCondition == null || card.Aspects.HasAspects(aspectsAsCondition)
                where this.ForbiddenAspects?.Intersect(card.Aspects.Keys).Any() != true
                where !this.IsUnique.HasValue || card.IsUnique == this.IsUnique.Value
                select card;

            // Sort for age bias.
            if (this.AgeBias.HasValue)
            {
                if (this.AgeBias == CardAgeSelection.Oldest)
                {
                    candidates = candidates.OrderBy(card => card.LifetimeRemaining);
                }
                else if (this.AgeBias == CardAgeSelection.Youngest)
                {
                    candidates = candidates.OrderByDescending(card => card.LifetimeRemaining);
                }
            }
            else
            {
                candidates = candidates.OrderBy(card => card.Aspects.GetWeight());
            }

            return candidates.FirstOrDefault();
        }
    }
}
