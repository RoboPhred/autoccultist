using System.Collections.Generic;
using Autoccultist.src.Brain.Util;

namespace Autoccultist.Brain.Config
{
    public class CardSetCondition : List<CardChoice>, IGameStateConditionConfig, IBaseCondition
    {
        public void Validate()
        {
            if (this == null || this.Count == 0)
            {
                throw new InvalidConfigException("CardSet must have card choices.");
            }
        }

        public bool IsConditionMet(IGameState state)
        {
            return state.CardsCanBeSatisfied(this);
        }
    }
}