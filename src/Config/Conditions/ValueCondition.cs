namespace AutoccultistNS.Config.Conditions
{
    using System;
    using System.Collections.Generic;
    using AutoccultistNS.Config.Values;
    using AutoccultistNS.GameState;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Specifies a comparison against a time value.
    /// </summary>
    public class ValueCondition : ConfigObject, IYamlConvertible, IValueCondition
    {
        /// <summary>
        /// Gets or sets a value indicating that the target value must be greater than this amount.
        /// </summary>
        public IValueProviderConfig GreaterThan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the target value must be greater or equal to this amount.
        /// </summary>
        /// <remarks>
        /// This exists mainly for the shorthand number assignment.
        /// </remarks>
        public IValueProviderConfig GreaterThanOrEqualTo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the target value must be less than this amount.
        /// </summary>
        public IValueProviderConfig LessThan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the target value must be less than or equal to this amount.
        /// </summary>
        public IValueProviderConfig LessThanOrEqualTo { get; set; }

        /// <inheritdoc/>
        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (this.GreaterThan == null && this.LessThan == null && this.GreaterThanOrEqualTo == null && this.LessThanOrEqualTo == null)
            {
                throw new InvalidConfigException("Value condition must specify at least one of: greaterThan, greaterThanOrEqualTo, lessThan, lessThanOrEqualTo");
            }
        }

        /// <summary>
        /// Determines whether this comparison is true or false in regards to the given value.
        /// </summary>
        /// <param name="value">The value to run the comparison against.</param>
        /// <returns>True if the comparison is true for the given value, or False otherwise.</returns>
        public bool IsConditionMet(float value, IGameState state)
        {
            if (this.GreaterThan != null && value <= this.GreaterThan.GetValue(state))
            {
                return false;
            }

            if (this.GreaterThanOrEqualTo != null && value < this.GreaterThanOrEqualTo.GetValue(state))
            {
                return false;
            }

            if (this.LessThan != null && value >= this.LessThan.GetValue(state))
            {
                return false;
            }

            if (this.LessThanOrEqualTo != null && value > this.LessThanOrEqualTo.GetValue(state))
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            var content = new List<string>();
            if (this.GreaterThan != null)
            {
                content.Add($"> {this.GreaterThan.ToString()}");
            }

            if (this.GreaterThanOrEqualTo != null)
            {
                content.Add($">= {this.GreaterThanOrEqualTo.ToString()}");
            }

            if (this.LessThan != null)
            {
                content.Add($"< {this.LessThan.ToString()}");
            }

            if (this.LessThanOrEqualTo != null)
            {
                content.Add($"<= {this.LessThanOrEqualTo.ToString()}");
            }

            return $"({string.Join(", ", content)})";
        }

        /// <inheritdoc/>
        void IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            if (parser.TryConsume<Scalar>(out var scalar))
            {
                if (!float.TryParse(scalar.Value, out var value))
                {
                    throw new YamlException(scalar.Start, scalar.End, "ValueComparison must be an object or a floating point value.");
                }

                if (value > 0)
                {
                    this.GreaterThanOrEqualTo = new StaticValueProviderConfig(value);
                }
                else if (value == 0)
                {
                    this.GreaterThanOrEqualTo = new StaticValueProviderConfig(0);
                    this.LessThanOrEqualTo = new StaticValueProviderConfig(0);
                }
                else
                {
                    this.LessThan = new StaticValueProviderConfig(-value);
                }
            }
            else
            {
                // We cannot deserialize our own type, as that will re-call IYamlConvertible.Read
                // We use a seperate object without the interface to read the object form from.
                var objectForm = (ValueComparisonObject)nestedObjectDeserializer(typeof(ValueComparisonObject));
                this.GreaterThanOrEqualTo = objectForm.GreaterThanOrEqualTo;
                this.GreaterThan = objectForm.GreaterThan;
                this.LessThanOrEqualTo = objectForm.LessThanOrEqualTo;
                this.LessThan = objectForm.LessThan;
            }
        }

        /// <inheritdoc/>
        void IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The object form of a ValueComparison.  Used in YAML parsing.
        /// </summary>
        private class ValueComparisonObject
        {
            /// <summary>
            /// Gets or sets a value indicating that the target value must be greater than this amount.
            public IValueProviderConfig GreaterThan { get; set; }

            /// <summary>
            /// Gets or sets a value indicating that the target value must be greater or equal to this amount.
            /// </summary>
            public IValueProviderConfig GreaterThanOrEqualTo { get; set; }

            /// <summary>
            /// Gets or sets a value indicating that the target value must be less than this amount.
            /// </summary>
            public IValueProviderConfig LessThan { get; set; }

            /// <summary>
            /// Gets or sets a value indicating that the target value must be less than or equal to this amount.
            /// </summary>
            public IValueProviderConfig LessThanOrEqualTo { get; set; }
        }
    }
}
