namespace Autoccultist.Brain
{
    /// <summary>
    /// Describes an impulse to perform an operation.
    /// </summary>
    public interface IImpulse
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
        /// Gets the condition which must be met before this impulse can activate, if any.
        /// </summary>
        IGameStateCondition Requirements { get; }

        /// <summary>
        /// Gets the condition on which to prevent this impulse from activating, if any.
        /// </summary>
        IGameStateCondition Forbidders { get; }

        /// <summary>
        /// Gets the operation to perform when this impulse is triggered.
        /// </summary>
        IOperation Operation { get; }
    }
}
