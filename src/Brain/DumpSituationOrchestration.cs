namespace Autoccultist.Brain
{
    using System;
    using System.Collections.Generic;
    using Autoccultist.Actor;
    using Autoccultist.Actor.Actions;

    /// <summary>
    /// An orchestration that dumps all cards from its situation.
    /// </summary>
    public class DumpSituationOrchestration : ISituationOrchestration
    {
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
        public void Start()
        {
            this.DumpSituation();
        }

        /// <inheritdoc/>
        public void Update()
        {
            // Nothing to do here, waiting on the Actor to finish the dump.
        }

        private async void DumpSituation()
        {
            try
            {
                await AutoccultistActor.PerformActions(this.DumpSituationCoroutine());
            }
            catch (Exception ex)
            {
                AutoccultistPlugin.Instance.LogWarn($"Failed to dump situation {this.SituationId}: {ex.Message}");
            }
            finally
            {
                this.Completed?.Invoke(this, EventArgs.Empty);
            }
        }

        private IEnumerable<IAutoccultistAction> DumpSituationCoroutine()
        {
            yield return new OpenSituationAction(this.SituationId);
            yield return new DumpSituationAction(this.SituationId);

            // This action may fail if the situation no longer exists.
            //  This happens with transient situations like suspicion.
            yield return new CloseSituationAction(this.SituationId)
            {
                IgnoreFailures = true,
            };
        }
    }
}
