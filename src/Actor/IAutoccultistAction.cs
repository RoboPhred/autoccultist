namespace AutoccultistNS.Actor
{
    /// <summary>
    /// An action that can be executed by the Actor.
    /// </summary>
    public interface IAutoccultistAction
    {
        /// <summary>
        /// Execute the action.
        /// </summary>
        /// <returns>The result of the action.</returns>
        ActionResult Execute();
    }
}
