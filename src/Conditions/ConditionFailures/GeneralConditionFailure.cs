namespace AutoccultistNS
{
    public class GeneralConditionFailure : ConditionFailure
    {
        public GeneralConditionFailure(string reason)
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
