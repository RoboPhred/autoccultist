namespace AutoccultistNS
{
    public class GameStateConditionFailure : ConditionResult
    {
        public GameStateConditionFailure(IGameStateCondition condition, ConditionResult unexpectedResult)
            : base(false)
        {
            this.Condition = condition;
            this.UnexpectedResult = unexpectedResult;
        }

        public IGameStateCondition Condition { get; }

        public ConditionResult UnexpectedResult { get; }

        public override string ToString()
        {
            return $"Unexpected result for condition {this.Condition}: {this.UnexpectedResult}";
        }
    }
}
