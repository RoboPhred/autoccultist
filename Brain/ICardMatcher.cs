using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;

namespace Autoccultist.Brain
{
    public interface ICardMatcher
    {
        bool CardMatches(IElementStack card);
    }

    public static class CardMatcher
    {
        public static IElementStack GetMatch(this ICardMatcher cardMatcher, ICollection<IElementStack> cards)
        {
            return cards.FirstOrDefault(x => cardMatcher.CardMatches(x));
        }
    }
}