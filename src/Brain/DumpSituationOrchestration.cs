namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using AutoccultistNS.Actor;
    using AutoccultistNS.Actor.Actions;

    /// <summary>
    /// An orchestration that dumps all cards from its situation.
    /// </summary>
    public class DumpSituationOrchestration : ISituationOrchestration
    {
        private CancellationTokenSource currentTask;

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
            this.currentTask?.Cancel();
            this.currentTask = null;
        }

        private async void DumpSituation()
        {
            try
            {
                this.currentTask = new CancellationTokenSource();
                await AutoccultistActor.PerformActions(this.DumpSituationCoroutine(), this.currentTask.Token);
            }
            catch (Exception ex)
            {
                NoonUtility.LogWarning(ex, $"Failed to dump situation {this.SituationId}: {ex.Message}");
            }
            finally
            {
                this.Completed?.Invoke(this, EventArgs.Empty);
            }
        }

        private IEnumerable<IAutoccultistAction> DumpSituationCoroutine()
        {
            yield return new OpenSituationAction(this.SituationId);
            yield return new EmptySituationAction(this.SituationId);
            yield return new CloseSituationAction(this.SituationId);
        }
    }
}
