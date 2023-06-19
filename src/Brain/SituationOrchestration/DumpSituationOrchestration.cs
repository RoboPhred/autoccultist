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
        public Task Start()
        {
            return this.DumpSituation();
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
            this.cancellationToken = null;
        }

        private async Task DumpSituation()
        {
            try
            {
                this.cancellationToken = new CancellationTokenSource();
                await AutoccultistActor.Perform(this.DumpSituationCoroutine, this.cancellationToken.Token);
            }
            catch (Exception ex)
            {
                NoonUtility.LogWarning(ex, $"Failed to dump situation {this.SituationId}: {ex.Message}");
            }
            finally
            {
                this.cancellationToken = null;
                this.Completed?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task DumpSituationCoroutine(CancellationToken cancellationToken)
        {
            await new ConcludeSituationAction(this.SituationId).ExecuteAndWait(cancellationToken);
        }
    }
}
