namespace AutoccultistNS
{
    using System.Collections.Generic;
    using System.Linq;

    public class CompoundConditionFailure : ConditionFailure
    {
        public CompoundConditionFailure(IReadOnlyCollection<ConditionFailure> failures)
        {
            this.Failures = failures;
        }

        public IReadOnlyCollection<ConditionFailure> Failures { get; private set; }

        public override string ToString()
        {
            return string.Join("\n", this.Failures.Select(f => f.ToString()));
        }
    }
}
