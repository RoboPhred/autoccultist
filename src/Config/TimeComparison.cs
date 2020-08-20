namespace Autoccultist.Config
{
    /// <summary>
    /// Specifies a comparison against a time value.
    /// </summary>
    public class TimeComparison : IConfigObject
    {
        /// <summary>
        /// Gets or sets a value indicating that the target value must be greater than this amount.
        /// <para>
        /// Only <see cref="GreaterThan"/> or <see cref="LessThan"/> can be specified at once, not both.
        /// </summary>
        public float? GreaterThan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the target value must be less than this amount.
        /// <para>
        /// Only <see cref="GreaterThan"/> or <see cref="LessThan"/> can be specified at once, not both.
        /// </summary>
        public float? LessThan { get; set; }

        /// <inheritdoc/>
        public void Validate()
        {
            if (this.GreaterThan.HasValue == this.LessThan.HasValue)
            {
                throw new InvalidConfigException("Time comparison must either have a greaterThan or lessThan, but not both.");
            }
        }

        /// <summary>
        /// Determines whether this comparison is true or false in regards to the given value.
        /// </summary>
        /// <param name="value">The value to run the comparison against.</param>
        /// <returns>True if the comparison is true for the given value, or False otherwise.</returns>
        public bool IsComparisonTrue(float value)
        {
            if (this.GreaterThan.HasValue)
            {
                return value > this.GreaterThan.Value;
            }

            if (this.LessThan.HasValue)
            {
                return value < this.LessThan.Value;
            }

            return false;
        }
    }
}
