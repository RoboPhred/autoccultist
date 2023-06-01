namespace AutoccultistNS.Config.Conditions
{
    using System;
    using System.Collections.Generic;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Specifies a comparison against a time value.
    /// </summary>
    public class ValueCondition : IConfigObject, IYamlConvertible, IValueCondition, IAfterYamlDeserialization
    {
        /// <summary>
        /// Gets or sets a value indicating that the target value must be greater than this amount.
        /// </summary>
        public float? GreaterThan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the target value must be greater or equal to this amount.
        /// </summary>
        /// <remarks>
        /// This exists mainly for the shorthand number assignment.
        /// </remarks>
        public float? GreaterThanOrEqualTo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the target value must be less than this amount.
        /// </summary>
        public float? LessThan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the target value must be less than or equal to this amount.
        /// </summary>
        public float? LessThanOrEqualTo { get; set; }

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (!this.GreaterThan.HasValue && !this.LessThan.HasValue && !this.GreaterThanOrEqualTo.HasValue && !this.LessThanOrEqualTo.HasValue)
            {
                throw new InvalidConfigException("Value condition must specify at least one of: greaterThan, greaterThanOrEqualTo, lessThan, lessThanOrEqualTo");
            }
        }

        /// <summary>
        /// Determines whether this comparison is true or false in regards to the given value.
        /// </summary>
        /// <param name="value">The value to run the comparison against.</param>
        /// <returns>True if the comparison is true for the given value, or False otherwise.</returns>
        public bool IsConditionMet(float value)
        {
            if (this.GreaterThan.HasValue && value <= this.GreaterThan.Value)
            {
                return false;
            }

            if (this.GreaterThanOrEqualTo.HasValue && value < this.GreaterThanOrEqualTo.Value)
            {
                return false;
            }

            if (this.LessThan.HasValue && value >= this.LessThan.Value)
            {
                return false;
            }

            if (this.LessThanOrEqualTo.HasValue && value > this.LessThanOrEqualTo.Value)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            var content = new List<string>();
            if (this.GreaterThan.HasValue)
            {
                content.Add($"> {this.GreaterThan.Value}");
            }

            if (this.GreaterThanOrEqualTo.HasValue)
            {
                content.Add($">= {this.GreaterThanOrEqualTo.Value}");
            }

            if (this.LessThan.HasValue)
            {
                content.Add($"< {this.LessThan.Value}");
            }

            return $"ValueCondition({string.Join(", ", content)})";
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
                    this.GreaterThanOrEqualTo = value;
                }
                else if (value == 0)
                {
                    this.GreaterThanOrEqualTo = 0;
                    this.LessThanOrEqualTo = 0;
                }
                else
                {
                    this.LessThan = -value;
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
            nestedObjectSerializer(this, this.GetType());
        }

        /// <summary>
        /// The object form of a ValueComparison.  Used in YAML parsing.
        /// </summary>
        private class ValueComparisonObject
        {
            /// <summary>
            /// Gets or sets a value indicating that the target value must be greater than this amount.
            public float? GreaterThan { get; set; }

            /// <summary>
            /// Gets or sets a value indicating that the target value must be greater or equal to this amount.
            /// </summary>
            public float? GreaterThanOrEqualTo { get; set; }

            /// <summary>
            /// Gets or sets a value indicating that the target value must be less than this amount.
            /// </summary>
            public float? LessThan { get; set; }

            /// <summary>
            /// Gets or sets a value indicating that the target value must be less than or equal to this amount.
            /// </summary>
            public float? LessThanOrEqualTo { get; set; }
        }
    }
}
