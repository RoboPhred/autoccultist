using System;
using System.Collections.Generic;
using Autoccultist.Actor;
using Autoccultist.Actor.Actions;

namespace Autoccultist.Brain
{
    class DumpSituationOrchestration : ISituationOrchestration
    {
        public event EventHandler Completed;

        public string SituationId { get; private set; }

        public DumpSituationOrchestration(string situationId)
        {
            this.SituationId = situationId;
        }

        public void Start()
        {
            this.DumpSituation();
        }

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
                if (this.Completed != null)
                {
                    this.Completed(this, EventArgs.Empty);
                }
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
                IgnoreFailures = true
            };
        }
    }
}