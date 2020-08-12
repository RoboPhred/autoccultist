using Autoccultist.Brain.Util;
using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    /**
     * A Goal represents a set of actions that can be satisfied by a given board state.
     * A Goal contains a set of Imperatives that will presumably cause the board to reach a win state.
     */
    public class Goal : ITask
    {
        public string Name { get; set; }

        public GameStateCondition RequiredCards { get; set; }
        public GameStateCondition CompletedByCards { get; set; }

        public List<Imperative> Imperatives { get; set; }

        public bool ShouldExecute(IGameState state)
        {
            return CanActivate(state);
        }

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