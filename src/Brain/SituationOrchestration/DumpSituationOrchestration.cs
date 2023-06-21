namespace AutoccultistNS.Brain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Actor;
    using AutoccultistNS.Actor.Actions;

    /// <summary>
    /// An orchestration that dumps all cards from its situation.
    /// </summary>
    public class DumpSituationOrchestration : ISituationOrchestration
    {
        private CancellationTokenSource cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="DumpSituationOrchestration"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to dump.</param>
        public DumpSituationOrchestration(string situationId)
        {
            this.SituationId = situationId;
        }

        /// <inheritdoc/>
        public event EventHandler Completed;

        /// <inheritdoc/>
        public string SituationId { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[DumpSituationOrchestration Situation={this.SituationId}]";
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (this.cancellationToken != null)
            {
                throw new InvalidOperationException("Cannot execute a dump situation orchestration more than once.");
            }

            this.DumpSituation();
        }

        /// <inheritdoc/>
        public void Update()
        {
            // Nothing to do here, waiting on the Actor to finish the dump.
        }

        /// <inheritdoc/>
        public void Abort()
        {
            this.cancellationToken?.Cancel();
        }

        private async void DumpSituation()
        {
            try
            {
                this.cancellationToken = new CancellationTokenSource();
                await Cerebellum.Coordinate(this.DumpSituationCoroutine, this.cancellationToken.Token);
            }
            catch (Exception ex)
            {
                Autoccultist.LogWarn(ex, $"Failed to dump situation {this.SituationId}: {ex.Message}");
            }
            finally
            {
                this.Completed?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task DumpSituationCoroutine(CancellationToken cancellationToken)
        {
            // While we don't technically need to be paused for this, we are taking time away from other
            // Cerebellum actions, so we should pause to not let game state decay too badly.
            var pauseToken = GameAPI.Pause();
            try
            {
                await new ConcludeSituationAction(this.SituationId).ExecuteAndWait(cancellationToken);
            }
            finally
            {
                // Might get op cancelled exception.
                pauseToken.Dispose();
            }
        }
    }
}
