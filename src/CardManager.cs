namespace Autoccultist
{
    using System.Collections.Generic;
    using System.Linq;
    using Assets.Core.Interfaces;

    /// <summary>
    /// Static classes for dealing with cards in the game.
    /// </summary>
    public static class CardManager
    {
        /// <summary>
        /// Determines if all card matchers can be satisfied.
        /// Satisfied matches removes the card from consideration.
        /// </summary>
        /// <param name="choices">The collection of card matchers to satisfy.</param>
        /// <returns>True if all matchers in the collection have a unique card that can be satisfied.</returns>
        public static bool CardsCanBeSatisfied(IReadOnlyCollection<ICardMatcher> choices)
        {
            var cards = GetAvailableCards().ToList();
            foreach (var choice in choices)
            {
                var match = choice.GetMatch(cards);
                if (match == null)
                {
                    return false;
                }

                // Remove the match so it is not double counted.
                cards.Remove(match);
            }

            return true;
        }

        /// <summary>
        /// Choose a card that satisfies the matcher.
        /// </summary>
        /// <param name="choice">The card matcher to choose a card with.</param>
        /// <returns>The chosen card, or null if none was found.</returns>
        public static IElementStack ChooseCard(ICardMatcher choice)
        {
            var cards = GetAvailableCards();
            var match = choice.GetMatch(cards);
            return match;
        }

        /// <summary>
        /// Gets all cards available for use.
        /// This excludes cards inside situations.
        /// </summary>
        /// <returns>An enumerable of all cards available for use.</returns>
        private static IEnumerable<IElementStack> GetAvailableCards()
        {
            // TODO: take into account card reservations
            // Order lifetime ascending so cards closer to expiration get chosen first.
            return GameAPI.GetTabletopCards().OrderBy(x => x.LifetimeRemaining);
        }
    }
}
