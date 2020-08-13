using System;
using System.Collections.Generic;
using System.Linq;
using Autoccultist.GameState;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Autoccultist.Brain.Config
{
    public class CardCondition : IYamlConvertible
    {
        // The trend in yaml is to use properties for mode selectors.
        //  See kubernetes and docker compose files.

        public static CardCondition NeedsAllOf(params CardChoice[] requirements)
        {
            return new CardCondition(ConditionMode.AllOf, requirements);
        }

        public static CardCondition NeedsAnyOf(params CardChoice[] requirements)
        {
            return new CardCondition(ConditionMode.AnyOf, requirements);
        }

        public static CardCondition NeedsNoneOf(params CardChoice[] requirements)
        {
            return new CardCondition(ConditionMode.NoneOf, requirements);
        }

        public ConditionMode Mode { get; set; }
        public List<CardChoice> Requirements { get; set; }

        public CardCondition()
        {
        }

        public CardCondition(ConditionMode mode, params CardChoice[] requirements)
        {
            this.Mode = mode;
            this.Requirements = new List<CardChoice>(requirements);
        }

        public bool IsConditionMet(IGameState state)
        {
            // Try to consume from the child scope, and see if we can consume all of them.
            //  The child scope's consumptions will not affect the parent, as to not pollute
            //  sibling and ancestor GameStateConditions
            var childScope = state.CreateConsumptionScope();

            switch (this.Mode)
            {
                case ConditionMode.AllOf:
                    return this.Requirements.All(card => card.TryConsume(childScope, out var _));
                case ConditionMode.AnyOf:
                    return this.Requirements.Any(card => card.TryConsume(childScope, out var _));
                case ConditionMode.NoneOf:
                    return !this.Requirements.Any(card => card.TryConsume(childScope, out var _));
                default:
                    throw new NotImplementedException($"Condition mode {this.Mode} is not implemented.");
            }
        }

        void IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            parser.Consume<MappingStart>();

            var key = parser.Consume<Scalar>();
            switch (key.Value)
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

            this.Requirements = (List<CardChoice>)nestedObjectDeserializer(typeof(List<CardChoice>));

            if (parser.Accept<Scalar>(out var _))
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