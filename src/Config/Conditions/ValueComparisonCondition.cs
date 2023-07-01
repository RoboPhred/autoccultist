namespace AutoccultistNS.Config.Conditions
{
    using System.Collections.Generic;
    using AutoccultistNS.Config.Values;
    using AutoccultistNS.GameState;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Specifies a comparison against a time value.
    /// </summary>
    public class ValueComparisonCondition : ConditionConfig
    {
        /// <summary>
        /// Gets or sets the value to compare against.
        /// </summary>
        public IValueProviderConfig Value { get; set; }

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

        /// <inheritdoc/>
        public override ConditionResult IsConditionMet(IGameState state)
        {
            var value = this.Value.GetValue(state);

            if (this.GreaterThan != null && value <= this.GreaterThan.GetValue(state))
            {
                return AddendedConditionResult.Addend(ConditionResult.Failure, $"Value {value} was not greater than {this.GreaterThan.GetValue(state)} ({this.Value} > {this.GreaterThan})");
            }

            if (this.GreaterThanOrEqualTo != null && value < this.GreaterThanOrEqualTo.GetValue(state))
            {
                return AddendedConditionResult.Addend(ConditionResult.Failure, $"Value {value} was not greater than or equal to {this.GreaterThanOrEqualTo.GetValue(state)} ({this.Value} >= {this.GreaterThanOrEqualTo})");
            }

            if (this.LessThan != null && value >= this.LessThan.GetValue(state))
            {
                return AddendedConditionResult.Addend(ConditionResult.Failure, $"Value {value} was not less than {this.LessThan.GetValue(state)} ({this.Value} < {this.LessThan})");
            }

            if (this.LessThanOrEqualTo != null && value > this.LessThanOrEqualTo.GetValue(state))
            {
                return AddendedConditionResult.Addend(ConditionResult.Failure, $"Value {value} was not less than or equal to {this.LessThanOrEqualTo.GetValue(state)} ({this.Value} <= {this.LessThanOrEqualTo})");
            }

            return ConditionResult.Success;
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
    }
}
