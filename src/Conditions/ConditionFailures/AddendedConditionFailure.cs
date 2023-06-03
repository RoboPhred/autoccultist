namespace AutoccultistNS
{
    public class AddendedConditionFailure : ConditionResult
    {
        public AddendedConditionFailure(ConditionResult failure, string addendum)
            : base(false)
        {
            this.Failure = failure;
            this.Addendum = addendum;
        }

        public ConditionResult Failure { get; private set; }

        public string Addendum { get; private set; }

        public override string ToString()
        {
            return $"{this.Failure} {this.Addendum}";
        }
    }
}
