namespace AutoccultistNS
{
    public class AddendedConditionFailure : ConditionFailure
    {
        public AddendedConditionFailure(ConditionFailure failure, string addendum)
        {
            this.Failure = failure;
            this.Addendum = addendum;
        }

        public ConditionFailure Failure { get; private set; }

        public string Addendum { get; private set; }

        public override string ToString()
        {
            return $"{this.Failure} {this.Addendum}";
        }
    }
}
