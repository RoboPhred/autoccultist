using System;
using System.Collections.Generic;
using System.Linq;

namespace Autoccultist.Brain.Config
{
    public class GameStateCondition
    {
        // The trend in yaml is to use properties for mode selectors.
        //  See kubernetes and docker compose files.

        public ConditionMode Mode { get; set; }
        public List<CardChoice> Requirements { get; set; }

        public GameStateCondition()
        {
        }

        public GameStateCondition(ConditionMode mode, params CardChoice[] requirements)
        {
            this.Mode = mode;
            this.Requirements = new List<CardChoice>(requirements);
        }

        public bool IsConditionMet(IGameState state)
        {
            switch (this.Mode)
            {
                case ConditionMode.AllOf:
                    return state.CardsCanBeSatisfied(this.Requirements);
                case ConditionMode.AnyOf:
                    return this.Requirements.Any(card => state.CardsCanBeSatisfied(new[] { card }));
                case ConditionMode.NoneOf:
                    return !this.Requirements.Any(card => state.CardsCanBeSatisfied(new[] { card }));
                default:
                    throw new NotImplementedException($"Condition mode {this.Mode} is not implemented.");
            }
        }

        public static GameStateCondition NeedsAllOf(params CardChoice[] requirements)
        {
            return new GameStateCondition(ConditionMode.AllOf, requirements);
        }

        public static GameStateCondition NeedsAnyOf(params CardChoice[] requirements)
        {
            return new GameStateCondition(ConditionMode.AnyOf, requirements);
        }

        public static GameStateCondition NeedsNoneOf(params CardChoice[] requirements)
        {
            return new GameStateCondition(ConditionMode.NoneOf, requirements);
        }
    }

    public enum ConditionMode
    {
        AnyOf,
        AllOf,
        NoneOf
    }
}