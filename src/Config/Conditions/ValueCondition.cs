namespace Autoccultist.Config.Conditions
{
    using System;
    using Autoccultist.Yaml;
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
        /// <para>
        /// Only <see cref="GreaterThan"/> or <see cref="LessThan"/> can be specified at once, not both.
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
        /// <para>
        /// Only <see cref="GreaterThan"/> or <see cref="LessThan"/> can be specified at once, not both.
        /// </summary>
        public float? LessThan { get; set; }

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (this.GreaterThan.HasValue && this.LessThan.HasValue && this.GreaterThan.Value > this.LessThan.Value)
            {
                throw new InvalidConfigException("Value comparison must either have a greaterThan, a lessThan, or have greaterThan be less than lessThan.");
            }

            if (this.GreaterThanOrEqualTo.HasValue && this.LessThan.HasValue && this.GreaterThanOrEqualTo.Value > this.LessThan.Value)
            {
                throw new InvalidConfigException("If greaterThanOrEqualTo and lessThan are specified, greaterThanOrEqualTo must be less than lessThan.");
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

            return true;
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

                if (value >= 0)
                {
                    this.GreaterThanOrEqualTo = value;
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
            /// <para>
            /// Only <see cref="GreaterThan"/> or <see cref="LessThan"/> can be specified at once, not both.
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
            /// <para>
            /// Only <see cref="GreaterThan"/> or <see cref="LessThan"/> can be specified at once, not both.
            /// </summary>
            public float? LessThan { get; set; }
        }
    }
}
