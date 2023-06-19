namespace AutoccultistNS.Brain
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Identifies a class that orchestrates the behavior of a situation.
    /// </summary>
    public interface ISituationOrchestration
    {
        /// <summary>
        /// Raised when this orchestration completes.
        /// </summary>
        event EventHandler Completed;

        /// <summary>
        /// Gets the situation id this orchestration is targeting.
        /// </summary>
        string SituationId { get; }

        /// <summary>
        /// Start the orchestration operating.
        /// </summary>
        Task Start();

        // FIXME: Remove this and rely on internal updates
        /// <summary>
        /// Allow the orchestration to perform its on-update tasks.
        /// </summary>
        /// <param name="gameState">The current game state.</param>
        void Update();

        /// <summary>
        /// Abort the orchestration.
        /// </summary>
        /// <param name="gameState">The current game state.</param>
        void Abort();
    }
}