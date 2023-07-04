namespace AutoccultistNS.Brain
{
    /// <summary>
    /// Defines a source of reactions
    /// </summary>
    /// <remarks>
    /// <see cref="IGameStateCondition"> should be implemented not as a check for what conditions an imperative wants to run this goal, but instead
    /// as a check for if this reaction can run at all.  In short, it should be limited to only tracking the resources necessary for this reaction to start.
    /// </remarks>
    public interface IReactor : IGameStateCondition
    {
        /// <summary>
        /// Gets the reaction.
        /// </summary>
        IReaction GetReaction();
    }
}
