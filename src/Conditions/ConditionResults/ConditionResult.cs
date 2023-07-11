namespace AutoccultistNS
{
    using System;

    /// <summary>
    /// The result of a condition check.
    /// </summary>
    public class ConditionResult : IEquatable<ConditionResult>, IComparable<ConditionResult>
    {
        /// <summary>
        /// A condition result that indicates success without further context.
        /// </summary>
        public static readonly ConditionResult Success = new ConditionResult(true);

        /// <summary>
        /// A condition result that indicates failure without further context.
        /// </summary>
        public static readonly ConditionResult Failure = new ConditionResult(false);

        private static int traceDepth = 0;

        public ConditionResult(bool result)
        {
            this.IsConditionMet = result;
        }

        public static bool IsTracing => traceDepth > 0;

        public bool IsConditionMet { get; }

        public static implicit operator bool(ConditionResult result) => result.IsConditionMet;

        /// <summary>
        /// Executes the function with condition tracing enabled.
        /// </summary>
        public static ConditionResult Trace(Func<ConditionResult> func)
        {
            var cacheWasEnabled = CacheUtils.Enabled;
            CacheUtils.Enabled = false;
            traceDepth++;
            try
            {
                return func();
            }
            finally
            {
                traceDepth--;
                CacheUtils.Enabled = cacheWasEnabled;
            }
        }

        /// <inheritdoc/>
        public override string ToString() => this.IsConditionMet ? "Condition Succeeded" : "Condition Failure";

        /// <inheritdoc/>
        public bool Equals(ConditionResult other)
        {
            return this.IsConditionMet == other.IsConditionMet;
        }

        public int CompareTo(ConditionResult other)
        {
            return this.IsConditionMet.CompareTo(other.IsConditionMet);
        }
    }
}
