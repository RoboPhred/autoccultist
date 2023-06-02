namespace AutoccultistNS
{
    public class SituationConditionFailure : ConditionFailure
    {
        public SituationConditionFailure(string situation, string addendum)
        {
            this.Situation = situation;
            this.Reason = addendum;
        }

        public string Situation { get; private set; }

        public string Reason { get; private set; }

        public override string ToString()
        {
            return $"{this.Situation} {this.Reason}";
        }
    }
}
