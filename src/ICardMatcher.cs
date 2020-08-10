using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;

namespace Autoccultist
{
    public interface ICardMatcher
    {
        bool CardMatches(IElementStack card);
    }

    public static class CardMatcher
    {
        public static IElementStack GetMatch(this ICardMatcher cardMatcher, IEnumerable<IElementStack> cards)
        {
            return cards.FirstOrDefault(x => cardMatcher.CardMatches(x));
        }
    }
}