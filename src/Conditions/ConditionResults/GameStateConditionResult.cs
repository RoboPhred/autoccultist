namespace AutoccultistNS
{
    /// <summary>
    /// Describes a condition relating to a game state condition.
    /// </summary>
    public class GameStateConditionResult : GeneralConditionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameStateConditionResult"/> class.
        /// </summary>
        /// <param name="condition">The condition that failed.</param>
        /// <param name="reason">The reason the condition failed.</param>
        private GameStateConditionResult(IGameStateCondition condition, ConditionResult unexpectedResult)
            : base($"The condition {condition} {(unexpectedResult ? "passed" : "failed")} when it should not have.")
        {
            this.Condition = condition;
            this.UnexpectedResult = unexpectedResult;
        }

        /// <summary>
        /// Gets the condition that failed.
        /// </summary>
        public IGameStateCondition Condition { get; }

        /// <summary>
        /// Gets the reason the condition failed.
        /// </summary>
        public ConditionResult UnexpectedResult { get; }

        /// <summary>
        /// Gets a condition result describing a game state condition failure.
        /// </summary>
        public static ConditionResult ForFailure(IGameStateCondition condition, ConditionResult unexpectedResult)
        {
            if (ConditionResult.IsTracing)
            {
                return new GameStateConditionResult(condition, unexpectedResult);
            }

            return ConditionResult.Failure;
        }
    }
}
