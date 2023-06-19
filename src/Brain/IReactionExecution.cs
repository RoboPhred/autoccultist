namespace AutoccultistNS.Brain
{
    using System;
    using System.Threading.Tasks;

    public interface IReactionExecution
    {
        /// <summary>
        /// Raised when this reaciton execution completes.
        /// </summary>
        event EventHandler Completed;

        /// <summary>
        /// Abort the execution.
        /// </summary>
        void Abort();

        /// <summary>
        /// Returns a task that completes when this reaction is established and ongoing.
        /// </summary>
        Task AwaitStarted();
    }
}