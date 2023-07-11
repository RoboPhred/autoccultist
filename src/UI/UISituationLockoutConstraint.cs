namespace AutoccultistNS.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameResources;
    using AutoccultistNS.GameState;

    public class UISituationLockoutConstraint : IResourceConstraint<ISituationState>
    {
        private bool isDisposed = false;

        public UISituationLockoutConstraint(string situationId)
        {
            this.SituationId = situationId;
        }

        public event EventHandler Disposed;

        public string SituationId { get; }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                this.Disposed?.Invoke(this, EventArgs.Empty);
            }
        }

        public IEnumerable<ISituationState> GetCandidates()
        {
            var situation = GameStateProvider.Current.Situations.FirstOrDefault(x => x.SituationId == this.SituationId);
            if (situation != null)
            {
                yield return situation;
            }
        }
    }
}
