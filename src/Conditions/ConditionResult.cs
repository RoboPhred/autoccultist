namespace AutoccultistNS
{
    public class ConditionResult
    {
        public static readonly ConditionResult Success = new ConditionResult(true);

        public ConditionResult(bool result)
        {
            this.IsConditionMet = result;
        }

        public bool IsConditionMet { get; }

        public static implicit operator bool(ConditionResult result) => result.IsConditionMet;
    }
}
