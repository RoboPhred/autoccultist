namespace Autoccultist
{
    using System.Collections.Generic;
    using System.Linq;
    using Assets.Core.Interfaces;

    /// <summary>
    /// Extension methods for <see cref="ICardMatcher"/>.
    /// </summary>
    public static class CardMatcherExtensions
    {
        /// <summary>
        /// Find a match from the given cards.
        /// </summary>
        /// <param name="cardMatcher">The card matcher to find a match with.</param>
        /// <param name="cards">The cards to find a match in.</param>
        /// <returns>The matching card, or null if none was found.</returns>
        public static IElementStack GetMatch(this ICardMatcher cardMatcher, IEnumerable<IElementStack> cards)
        {
            return cards.FirstOrDefault(x => cardMatcher.CardMatches(x));
        }
    }
}
