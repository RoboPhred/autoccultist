namespace AutoccultistNS
{
    using System.Collections.Generic;
    using System.Linq;

    public class CompoundConditionFailure : ConditionResult
    {
        public CompoundConditionFailure(IReadOnlyCollection<ConditionResult> failures)
        : base(false)
        {
            this.Failures = failures;
        }

        public IReadOnlyCollection<ConditionResult> Failures { get; private set; }

        public override string ToString()
        {
            return string.Join("\n", this.Failures.Select(f => f.ToString()));
        }
    }
}
