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
        /// <param name="cancellationToken">A CancellationToken used to cancel the action execution.</param>
        /// <returns>A boolean indicating whether the action performed any activity or not.</returns>
        Task<bool> Execute(CancellationToken cancellationToken);
    }
}
