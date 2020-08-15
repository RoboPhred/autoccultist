namespace Autoccultist.Brain.Config
{
    using System.Collections.Generic;
    using Assets.Core.Interfaces;

    /// <summary>
    /// A configuration node responsible for choosing a card based on its properties.
    /// </summary>
    public class CardChoice : IConfigObject, ICardMatcher, IGameStateConditionConfig
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

        /// <inheritdoc/>
        public void Validate()
        {
            if (string.IsNullOrEmpty(this.ElementId) && (this.Aspects == null || this.Aspects.Count == 0))
            {
                throw new InvalidConfigException("Card choice must have either an elementId or aspects.");
            }
        }

        /// <inheritdoc/>
        public bool CardMatches(IElementStack card)
        {
            if (this.ElementId != null && card.EntityId != this.ElementId)
            {
                return false;
            }

            if (this.Aspects != null)
            {
                var cardAspects = card.GetAspects();
                foreach (var aspectPair in this.Aspects)
                {
                    if (!cardAspects.TryGetValue(aspectPair.Key, out int cardAspect))
                    {
                        return false;
                    }

                    // For now, just looking for aspects that have at least that amount.
                    // TODO: We should be choosing the least matching card of all possible cards, to
                    //  leave higher aspect cards for other usages.
                    // May want to return a match weight where lower values get chosen over higher values
                    if (cardAspect < aspectPair.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public bool IsConditionMet(IGameState state)
        {
            return state.CardsCanBeSatisfied(new[] { this });
        }
    }
}
