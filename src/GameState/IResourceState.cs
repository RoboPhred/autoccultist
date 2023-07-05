namespace AutoccultistNS.GameState
{
    /// <summary>
    /// Indicates that this state object is tracked as a <see cref="AutoccultistNS.Resources.Resource"/>.
    /// </summary>
    public interface IResourceState
    {
        /// <summary>
        /// Returns a value indiciating whether this situation is available.
        /// Available resources are not not constrained by an existing resource constraint.
        /// </summary>
        bool IsAvailable { get; }
    }
}
