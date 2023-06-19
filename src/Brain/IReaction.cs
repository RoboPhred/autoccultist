namespace AutoccultistNS.Brain
{
    /// <summary>
    /// A reaction is an action that the bot conditionally wants to perform.
    /// When the condition is reached, the bot will perform its action.
    /// </summary>
    public interface IReaction : IGameStateCondition
    {
        /// <summary>
        /// Gets the priority of this reaction.
        /// </summary>
        TaskPriority Priority { get; }

        /// <summary>
        /// Perform the action associated with this reaction.
        /// </summary>
        IReactionExecution Execute();
    }
}
