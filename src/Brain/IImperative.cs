namespace Autoccultist.Brain
{
    using Autoccultist.Brain.Config;
    using Autoccultist.GameState;

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
        /// Gets a condition which must be met before this imperative can activate.
        /// </summary>
        IGameStateCondition Requirements { get; }

        /// <summary>
        /// Gets condition on which to prevent this imperative from activating.
        /// </summary>
        IGameStateCondition Forbidders { get; }

        /// <summary>
        /// Gets the operation to perform when this imperative is triggered.
        /// </summary>
        // TODO: Should be IOperation and unrelated to config.
        IOperation Operation { get; }
    }
}
