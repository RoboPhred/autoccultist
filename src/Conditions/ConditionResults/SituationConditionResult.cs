namespace AutoccultistNS
{
    public class SituationConditionResult : GeneralConditionResult
    {
        protected SituationConditionResult(string situation, string reason)
        : base($"Situation {situation} not in appropriate state: {reason}")
        {
            this.Situation = situation;
            this.Reason = reason;
        }

        public string Situation { get; private set; }

        public static ConditionResult ForFailure(string situation, string reason)
        {
            if (ConditionResult.IsTracing)
            {
                return new SituationConditionResult(situation, reason);
            }

            return ConditionResult.Failure;
        }
    }
}
