namespace AutoccultistNS.Brain
{
    /// <summary>
    /// Describes an impulse to perform an operation.
    /// </summary>
    public interface IImpulse : IGameStateCondition
    {
        /// <summary>
        /// Gets the human-friendly display name for this impulse.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the priority for this impulse.
        /// Impulses with a higher priority will run before lower priority impulses.
        /// </summary>
        TaskPriority Priority { get; }

        /// <summary>
        /// Gets the operation to perform when this impulse is triggered.
        /// </summary>
        IOperation Operation { get; }
    }
}
