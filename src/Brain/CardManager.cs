using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;

namespace Autoccultist.Brain
{
    public class CardManager
    {
        public bool CardsCanBeSatisfied(IReadOnlyCollection<ICardMatcher> choices)
        {
            var cards = this.GetAvailableCards().ToList();
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

        public IElementStack ChooseCard(ICardMatcher choice)
        {
            var cards = this.GetAvailableCards();
            var match = choice.GetMatch(cards);
            return match;
        }

        private IEnumerable<IElementStack> GetAvailableCards()
        {
            // TODO: take into account card reservations
            // Order lifetime ascending so cards closer to expiration get chosen first.
            return GameAPI.GetTabletopCards().OrderBy(x => x.LifetimeRemaining);
        }
    }
}