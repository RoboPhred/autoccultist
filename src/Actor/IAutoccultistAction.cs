namespace AutoccultistNS.Actor
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An action that can be executed by the Actor.
    /// </summary>
    public interface IAutoccultistAction
    {
        /// <summary>
        /// Execute the action.
        /// </summary>
        /// <param name="cancellationToken">An optional CancellationToken used to cancel the action execution.</param>
        /// <returns>The result of the action.</returns>
        Task<ActionResult> Execute(CancellationToken cancellationToken);
    }
}
