namespace AutoccultistNS
{
    /// <summary>
    /// Addends a <see cref="ConditionResult"/> with additional context.
    /// </summary>
    public class AddendedConditionResult : ConditionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddendedConditionResult"/> class.
        /// </summary>
        /// <param name="condition">The condition to addend.</param>
        /// <param name="addendum">The additional context to addend.</param>
        protected AddendedConditionResult(ConditionResult condition, string addendum)
            : base(condition.IsConditionMet)
        {
            this.InnerConditionResult = condition;
            this.Addendum = addendum;
        }

        /// <summary>
        /// Gets the condition that was addended.
        /// </summary>
        public ConditionResult InnerConditionResult { get; private set; }

        /// <summary>
        /// Gets the additional context that was addended.
        /// </summary>
        public string Addendum { get; private set; }

        /// <summary>
        /// Addends a <see cref="ConditionResult"/> with additional context, if tracing is enabled.
        /// </summary>
        public static ConditionResult Addend(ConditionResult condition, string addendum)
        {
            if (ConditionResult.IsTracing)
            {
                return new AddendedConditionResult(condition, addendum);
            }

            return condition;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.InnerConditionResult} {this.Addendum}";
        }
    }
}
