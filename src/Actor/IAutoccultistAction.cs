namespace AutoccultistNS.Actor
{
    /// <summary>
    /// An action that can be executed by the Actor.
    /// </summary>
    public interface IAutoccultistAction
    {
        /// <summary>
        /// Gets the unique id of this action.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Execute the action.
        /// </summary>
        void Execute();
    }
}
