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
            AutoccultistActor.PerformActions(this.DumpSituationCoroutine());
        }

        public void Update()
        {
            // Nothing to do here, waiting on the Actor to finish the dump.
        }

        private IEnumerable<IAutoccultistAction> DumpSituationCoroutine()
        {
            try
            {
                yield return new OpenSituationAction(this.SituationId);
                yield return new DumpSituationAction(this.SituationId);
                yield return new CloseSituationAction(this.SituationId);
            }
            finally
            {
                if (this.Completed != null)
                {
                    this.Completed(this, EventArgs.Empty);
                }
            }
        }
    }
}