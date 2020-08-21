namespace Autoccultist.Brain
{
    /// <summary>
    /// Describes an imperative to perform an operation.
    /// </summary>
    public interface IImperative
    {
        /// <summary>
        /// Gets the human-friendly display name for this imperative.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the priority for this imperative.
        /// Imperatives with a higher priority will run before lower priority imperatives.
        /// </summary>
        TaskPriority Priority { get; }

        /// <summary>
        /// Gets the condition which must be met before this imperative can activate, if any.
        /// </summary>
        IGameStateCondition Requirements { get; }

        /// <summary>
        /// Gets the condition on which to prevent this imperative from activating, if any.
        /// </summary>
        IGameStateCondition Forbidders { get; }

        /// <summary>
        /// Gets the operation to perform when this imperative is triggered.
        /// </summary>
        IOperation Operation { get; }
    }
}
