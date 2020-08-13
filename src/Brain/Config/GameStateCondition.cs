using System.Collections.Generic;
using Autoccultist.src.Brain.Util;

namespace Autoccultist.Brain.Config
{
    public class GameStateCondition : ICondition
    {
        // The trend in yaml is to use properties for mode selectors.
        //  See kubernetes and docker compose files.

        public ConditionMode mode { get; set; }
        public List<ICondition> Requirements { get; set; }


        public GameStateCondition()
        {
        }

        public GameStateCondition(ConditionMode Mode, params CardChoice[] requirements)
        {
            mode = Mode;
            Requirements = new List<ICondition>(requirements);
        }


        public bool IsConditionMet(IGameState state)
        {
            if(mode == ConditionMode.NONE_OF)
            {
                foreach (var condition in this.Requirements)
                {
                    if(condition.IsConditionMet(state))
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (mode == ConditionMode.ALL_OF)
            {
                foreach(var condition in this.Requirements)
                {
                    if(!condition.IsConditionMet(state))
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (mode == ConditionMode.ANY_OF)
            {
                foreach(var condition in this.Requirements)
                {
                    if(condition.IsConditionMet(state))
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