namespace AutoccultistNS
{
    public class GeneralConditionFailure : ConditionResult
    {
        public GeneralConditionFailure(string reason)
        : base(false)
        {
            this.Reason = reason;
        }

        public string Reason { get; set; }

        public override string ToString()
        {
            return this.Reason;
        }
    }
}
