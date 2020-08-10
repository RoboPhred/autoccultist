using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;

namespace Autoccultist
{
    public static class CardManager
    {
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

        public static IElementStack ChooseCard(ICardMatcher choice)
        {
            var cards = GetAvailableCards();
            var match = choice.GetMatch(cards);
            return match;
        }

        private static IEnumerable<IElementStack> GetAvailableCards()
        {
            // TODO: take into account card reservations
            // Order lifetime ascending so cards closer to expiration get chosen first.
            return GameAPI.GetTabletopCards().OrderBy(x => x.LifetimeRemaining);
        }
    }
}