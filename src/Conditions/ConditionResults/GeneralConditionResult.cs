namespace AutoccultistNS
{
    /// <summary>
    /// Describes a condition result.
    /// </summary>
    public class GeneralConditionResult : ConditionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralConditionResult"/> class.
        /// </summary>
        /// <param name="reason">The reason the condition failed.</param>
        protected GeneralConditionResult(string reason, bool result = false)
        : base(result)
        {
            this.Reason = reason;
        }

        /// <summary>
        /// Gets the reason the condition failed.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Gets a condition result describing a general failure.
        /// </summary>
        public static ConditionResult ForFailure(string reason)
        {
            if (ConditionResult.IsTracing)
            {
                return new GeneralConditionResult(reason);
            }

            return ConditionResult.Failure;
        }

        public override string ToString()
        {
            return $"{base.ToString()}: {this.Reason}";
        }
    }
}
