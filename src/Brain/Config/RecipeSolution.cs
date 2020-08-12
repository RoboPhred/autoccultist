using System.Collections.Generic;
using System.Linq;

namespace Autoccultist.Brain.Config
{
    public class RecipeSolution
    {
        public IDictionary<string, CardChoice> Slots;

        public bool CanExecute(IGameState state)
        {
            var cardMatchers = this.Slots.Values.Cast<ICardMatcher>().ToArray();
            if (!state.CardsCanBeSatisfied(cardMatchers))
            {
                return false;
            }
            return true;
        }
    }
}