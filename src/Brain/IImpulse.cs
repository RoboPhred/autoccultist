namespace AutoccultistNS.Brain
{
    /// <summary>
    /// An impulse is a condition dependent reaction that wants to execute as soon as its condition passes.
    /// Impulses have priority, and higher priority impulses will be executed before lower priority impulses.
    /// </summary>
    public interface IImpulse : IGameStateCondition, IReactor
    {
        /// <summary>
        /// Gets the priority of this impulse.
        /// </summary>
        TaskPriority Priority { get; }
    }
}
