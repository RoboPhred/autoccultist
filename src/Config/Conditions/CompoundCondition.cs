namespace AutoccultistNS.Config.Conditions
{
    using System;
    using System.Collections.Generic;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// A compound condition is a condition that can contain multiple child conditions, and
    /// require either all to match, at least one to match, or none to match.
    /// </summary>
    [DuckTypeKeys(new[] { "allOf", "anyOf", "noneOf" })]
    public class CompoundCondition : ConditionConfig, IYamlConvertible
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

        public override string ToString()
        {
            return $"CompoundCondition({this.Mode}, Name = \"{this.Name}\")";
        }

        /// <inheritdoc/>
        public override ConditionResult IsConditionMet(IGameState state)
        {
            var recordedFailures = new List<ConditionResult>();
            foreach (var condition in this.Requirements)
            {
                var matched = condition.IsConditionMet(state);
                switch (this.Mode)
                {
                    case ConditionMode.AllOf:
                        if (!matched)
                        {
                            return matched;
                        }

                        break;

                    case ConditionMode.AnyOf:
                        if (matched)
                        {
                            return ConditionResult.Success;
                        }

                        recordedFailures.Add(matched);
                        break;

                    case ConditionMode.NoneOf:
                        if (matched)
                        {
                            return GameStateConditionResult.ForFailure(condition, matched);
                        }

                        break;
                }
            }

            if (this.Mode == ConditionMode.AnyOf)
            {
                return CompoundConditionResult.ForFailure(recordedFailures);
            }

            return ConditionResult.Success;
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

        /// <inheritdoc/>
        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (this.Requirements == null || this.Requirements.Count == 0)
            {
                throw new InvalidConfigException("CompoundCondition must have requirements.");
            }
        }
    }
}
