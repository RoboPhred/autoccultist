namespace Autoccultist.Config.Conditions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autoccultist.GameState;
    using Autoccultist.Yaml;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// A compound condition is a condition that can contain multiple child conditions, and
    /// require either all to match, at least one to match, or none to match.
    /// </summary>
    [DuckTypeKeys(new[] { "allOf", "anyOf", "noneOf" })]
    public class CompoundCondition : IGameStateConditionConfig, IYamlConvertible, IAfterYamlDeserialization
    {
        /// <summary>
        /// A mode by which CompoundCondition resolves the relationship between its child conditions.
        /// </summary>
        public enum ConditionMode
        {
            /// <summary>
            /// Indicates any one of the child conditions must be true for the compound condition to be true
            /// </summary>
            AnyOf,

            /// <summary>
            /// Indicates all of the child conditions must be true for the compound condition to be true
            /// </summary>
            AllOf,

            /// <summary>
            /// Indicates none of the child conditions must be true for the compound condition to be true
            /// </summary>
            NoneOf,
        }

        /// <summary>
        /// Gets or sets the mode of this condition will aggregate its child conditions.
        /// </summary>
        public ConditionMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the list of child conditions within this compound condition.
        /// </summary>
        public List<IGameStateConditionConfig> Requirements { get; set; } = new List<IGameStateConditionConfig>();

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (this.Requirements == null || this.Requirements.Count == 0)
            {
                throw new InvalidConfigException("CompoundCondition must have requirements.");
            }
        }

        /// <inheritdoc/>
        public bool IsConditionMet(IGameState state)
        {
            switch (this.Mode)
            {
                case ConditionMode.AllOf:
                    return this.Requirements.All(condition => condition.IsConditionMet(state));
                case ConditionMode.AnyOf:
                    return this.Requirements.Any(condition => condition.IsConditionMet(state));
                case ConditionMode.NoneOf:
                    return !this.Requirements.Any(condition => condition.IsConditionMet(state));
                default:
                    throw new NotSupportedException($"Condition mode {this.Mode} is not implemented.");
            }
        }

        /// <inheritdoc/>
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
                    throw new YamlException(key.Start, key.End, "GameStateCondition must have one of the following keys: \"allOf\", \"anyOf\", \"noneOf\".");
            }

            this.Requirements = (List<IGameStateConditionConfig>)nestedObjectDeserializer(typeof(List<IGameStateConditionConfig>));

            if (parser.Accept<Scalar>(out var _))
            {
                throw new YamlException(key.Start, key.End, "GameStateCondition must only have one property.");
            }

            parser.Consume<MappingEnd>();
        }

        /// <inheritdoc/>
        void IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            throw new NotSupportedException();
        }
    }
}
