using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    /**
     * A Goal represents a set of actions that can be satisfied by a given board state.
     * A Goal contains a set of Imperatives that will presumably reach the given state.
     */
    public class Goal
    {
        public string Name { get; set; }

        public IGameStateConditionConfig Requirements { get; set; }
        public IGameStateConditionConfig CompletedWhen { get; set; }

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

            if (this.Requirements != null && !this.Requirements.IsConditionMet(state))
            {
                return false;
            }

            return true;
        }

        public bool IsSatisfied(IGameState state)
        {
            return this.CompletedWhen != null && this.CompletedWhen.IsConditionMet(state);
        }
    }
}