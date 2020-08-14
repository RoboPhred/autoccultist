using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    public class CardSetCondition : IGameStateCondition
    {
        public List<CardChoice> CardSet { get; set; } = new List<CardChoice>();

        public CardSetCondition()
        {
        }

        public bool IsConditionMet(IGameState state)
        {
            return state.CardsCanBeSatisfied(this.CardSet);
        }
    }
}