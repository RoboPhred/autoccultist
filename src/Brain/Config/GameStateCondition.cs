using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    public class GameStateCondition
    {
        // The trend in yaml is to use properties for mode selectors.
        //  See kubernetes and docker compose files.

        public ConditionMode mode { get; set; }
        public List<CardChoice> Requirements { get; set; }


        public GameStateCondition()
        {
        }

        public GameStateCondition(ConditionMode Mode, params CardChoice[] requirements)
        {
            mode = Mode;
            Requirements = new List<CardChoice>(requirements);
        }


        public bool IsConditionMet(IGameState state)
        {
            {
                return state.CardsCanBeSatisfied(this.AllOf);
            }

            if(mode == ConditionMode.NONE_OF)
            {
                foreach (var card in this.Requirements)
                {
                    if (state.CardsCanBeSatisfied(new[] { card }))
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (mode == ConditionMode.ALL_OF)
            {
                return state.CardsCanBeSatisfied(this.Requirements);
            }
            else if (mode == ConditionMode.ANY_OF)
            {
                foreach (var card in this.Requirements)
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

        public static GameStateCondition NeedsAllOf(params CardChoice[] requirements)
        {
            return new GameStateCondition(ConditionMode.ALL_OF, requirements);
        }

        public static GameStateCondition NeedsAnyOf(params CardChoice[] requirements)
        {
            return new GameStateCondition(ConditionMode.ANY_OF, requirements);
        }

        public static GameStateCondition NeedsNoneOf(params CardChoice[] requirements)
        {
            return new GameStateCondition(ConditionMode.NONE_OF, requirements);
        }
    }

    public enum ConditionMode
    {
        ANY_OF,
        ALL_OF,
        NONE_OF
    }
}