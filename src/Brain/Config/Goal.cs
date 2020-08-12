using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    public class Goal
    {
        public string Name { get; set; }

        public GameStateCondition RequiredCards { get; set; }
        public GameStateCondition CompletedByCards { get; set; }

        public List<Imperative> Imperatives { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new InvalidConfigException("Goal must have a name.");
            }
            if (this.Imperatives == null || this.Imperatives.Count == 0)
            {
                throw new InvalidConfigException("Goal must have an imperative");
            }

            foreach (var imperative in this.Imperatives)
            {
                imperative.Validate();
            }
        }

        public bool CanActivate(IGameState state)
        {
            if (this.IsSatisfied(state))
            {
                return false;
            }

            if (this.RequiredCards != null && !this.RequiredCards.IsConditionMet(state))
            {
                return false;
            }

            return true;
        }

        public bool IsSatisfied(IGameState state)
        {
            return this.CompletedByCards != null && this.CompletedByCards.IsConditionMet(state);
        }
    }
}