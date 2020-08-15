namespace Autoccultist
{
    using System.Collections.Generic;
    using System.Linq;
    using Assets.Core.Interfaces;

    public static class CardMatcherExtensions
    {
        public static IElementStack GetMatch(this ICardMatcher cardMatcher, IEnumerable<IElementStack> cards)
        {
            return cards.FirstOrDefault(x => cardMatcher.CardMatches(x));
        }
    }
}