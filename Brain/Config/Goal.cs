using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    public class Goal
    {
        public string Name { get; set; }

        public IGameStateCondition RequiredCards { get; set; }
        public IGameStateCondition CompletedByCards { get; set; }

        public List<Imperative> Imperatives { get; set; }

        public bool CanActivate(IGameState state)
        {
            if (this.IsSatisfied(state))
            {
                return false;
            }

            if (!this.RequiredCards.IsConditionMet(state))
            {
                return false;
            }

            return true;
        }

        public bool IsSatisfied(IGameState state)
        {
            return this.CompletedByCards.IsConditionMet(state);
        }
    }
}