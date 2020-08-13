using System;
using System.Collections.Generic;
using System.Linq;
using Autoccultist.src.Brain.Util;
using Autoccultist.src.Brain;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Autoccultist.Brain.Config
{
    public class GameStateCondition : IYamlConvertible, ICondition
    {
        // The trend in yaml is to use properties for mode selectors.
        //  See kubernetes and docker compose files.

        public ConditionMode Mode { get; set; }
        public List<ICondition> Requirements { get; set; }

        public GameStateCondition()
        {
        }

        public GameStateCondition(ConditionMode mode, params ICondition[] requirements)
        {
            this.Mode = mode;
            this.Requirements = new List<ICondition>(requirements);
        }

        public bool IsConditionMet(IGameState state)
        {
            switch(this.Mode)
            {
            case ConditionMode.AllOf:
                List<CardChoice> cardsRequired = new List<CardChoice>();
                foreach(ICondition condition in this.Requirements)
                {
                    cardsRequired.Add(condition as CardChoice);
                    if(condition is GameStateCondition)
                    {
                        cardsRequired.AddRange((condition as GameStateCondition).GetAllCardsNeeded(state));
                    }
                }
                return state.CardsCanBeSatisfied(cardsRequired);
            case ConditionMode.AnyOf:
                return this.Requirements.Any(condition => condition.IsConditionMet(state));
            case ConditionMode.NoneOf:
                return !this.Requirements.Any(condition => condition.IsConditionMet(state));
            default:
                throw new NotImplementedException($"Condition mode {this.Mode} is not implemented.");
            }
        }

        private IEnumerable<CardChoice> GetAllCardsNeeded(IGameState state)
        {
            switch(this.Mode)
            {
            case ConditionMode.AllOf:
                List<CardChoice> list = new List<CardChoice>();
                foreach(ICondition condition in Requirements)
                {
                    switch(condition)
                    {
                    case GameStateCondition _:
                        list.AddRange((condition as GameStateCondition).GetAllCardsNeeded(state));
                        break;
                    case CardChoice _:
                        list.Add(condition as CardChoice);
                        break;
                    }
                }
                return list;

            case ConditionMode.NoneOf:
                if(this.IsConditionMet(state))
                {
                    return new CardChoice[0];
                }

                throw new ConditionNotMetException("A 'NoneOf' requirement was not met: " + this.ToString());

            case ConditionMode.AnyOf:
                throw new NotImplementedException("AnyOf GameStateConditions are not (currently) allowed to be children of AllOf GameStateConditions.");

            default:
                throw new NotImplementedException($"Condition mode {this.Mode} is not implemented.");
            }
        }

        void IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            parser.Consume<MappingStart>();

            var key = parser.Consume<Scalar>();
            switch(key.Value)
            {
            case "allOf":
                this.Mode = ConditionMode.AllOf;
                break;
            case "anyOf":
                this.Mode = ConditionMode.AnyOf;
                break;
            case "noneOf":
                this.Mode = ConditionMode.NoneOf;
                break;
            default:
                throw new YamlException(key.Start, key.End, "GameStateCondition must have one of the following keys: \"allOf\", \"anyOf\", \"oneOf\".");
            }

            this.Requirements = new List<ICondition>((List<CardChoice>) nestedObjectDeserializer(typeof(List<CardChoice>)));

            if(parser.Accept<Scalar>(out var _))
            {
                throw new YamlException(key.Start, key.End, "GameStateCondition must only have one property.");
            }

            parser.Consume<MappingEnd>();
        }

        void IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            throw new NotSupportedException();
        }
    }

    public enum ConditionMode
    {
        AnyOf,
        AllOf,
        NoneOf
    }
}