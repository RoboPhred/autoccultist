using System.Collections.Generic;
using System.Linq;

namespace Autoccultist.Brain.Config
{
    public class CardsSatisfiedCondition : IGameStateCondition
    {
        public List<CardChoice> Cards { get; set; }

        public CardsSatisfiedMode Mode = CardsSatisfiedMode.All;

        public bool IsConditionMet(IGameState state)
        {
            if (this.Mode == CardsSatisfiedMode.All)
            {
                return state.CardsCanBeSatisfied(this.Cards);
            }
            else if (this.Mode == CardsSatisfiedMode.Any)
            {
                foreach (var card in this.Cards)
                {
                    if (state.CardsCanBeSatisfied(new[] { card }))
                    {
                        return true;
                    }
                }
                return false;
            }

            return false;
        }
    }

    public enum CardsSatisfiedMode
    {
        Any,
        All
    }
}