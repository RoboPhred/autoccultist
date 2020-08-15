namespace Autoccultist.Brain.Config.Conditions
{
    using System.Collections.Generic;
    using System.Linq;
    using Assets.Core.Interfaces;

    public class CardSetCondition : ICardCondition
    {
        /// <summary>
        /// Gets or sets a list of cards, all of which must be present at the same time to meet this condition.
        /// <para>
        /// Each choice in this list must match to a different card.  Once a card is chosen by a card choice,
        /// other card choices in the list will be unable to match that same card.
        /// </summary>
        public List<CardChoice> CardSet { get; set; } = new List<CardChoice>();

        /// <inheritdoc/>
        public void Validate()
        {
            if (this.CardSet == null || this.CardSet.Count == 0)
            {
                throw new InvalidConfigException("CardSet must have card choices.");
            }
        }

        /// <inheritdoc/>
        public bool IsConditionMet(IGameState state)
        {
            return state.CardsCanBeSatisfied(this.CardSet);
        }

        /// <inheritdoc/>
        public bool CardsMatchSet(IList<IElementStack> cards)
        {
            var remaining = new HashSet<IElementStack>(cards);
            foreach (var choice in this.CardSet)
            {
                var match = remaining.FirstOrDefault(card => choice.CardMatches(card));
                if (match == null)
                {
                    return false;
                }

                remaining.Remove(match);
            }

            return true;
        }
    }
}
