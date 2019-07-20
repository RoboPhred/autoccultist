using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    public class GameStateCondition
    {
        // The trend in yaml is to use properties for mode selectors.
        //  See kubernetes and docker compose files.

        public List<CardChoice> AllOf { get; set; }
        public List<CardChoice> AnyOf { get; set; }

        public bool IsConditionMet(IGameState state)
        {
            if (this.AllOf != null)
            {
                return state.CardsCanBeSatisfied(this.AllOf);
            }
            else if (this.AnyOf != null)
            {
                foreach (var card in this.AnyOf)
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
}