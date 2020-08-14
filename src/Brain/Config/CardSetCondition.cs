using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    public class CardSetCondition : IGameStateConditionConfig
    {
        public List<CardChoice> CardSet { get; set; }

        public void Validate()
        {
            if (this.CardSet == null || this.CardSet.Count == 0)
            {
                throw new InvalidConfigException("CardSet must have card choices.");
            }
        }

        public bool IsConditionMet(IGameState state)
        {
            return state.CardsCanBeSatisfied(this.CardSet);
        }
    }
}