namespace AutoccultistNS
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A condition result recording the combined result of several other conditions.
    /// </summary>
    public class CompoundConditionResult : GeneralConditionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundConditionResult"/> class.
        /// </summary>
        /// <param name="conditions">The conditions that failed.</param>
        protected CompoundConditionResult(IReadOnlyCollection<ConditionResult> conditions)
        : base($"None of the expected conditions succeeded: [{string.Join(", ", conditions.Select(f => f.ToString()))}]")
        {
            this.Conditions = conditions;
        }

        /// <summary>
        /// Gets the conditions that failed.
        /// </summary>
        public IReadOnlyCollection<ConditionResult> Conditions { get; private set; }

        /// <summary>
        /// Gets a condition result describing a compound failure.
        /// </summary>
        public static ConditionResult ForFailure(IEnumerable<ConditionResult> conditions)
        {
            if (ConditionResult.IsTracing)
            {
                return new CompoundConditionResult(conditions.ToList());
            }

            return ConditionResult.Failure;
        }
    }
}
