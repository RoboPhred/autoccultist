namespace AutoccultistNS.Actor.Actions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.GameState;
    using SecretHistories.Entities;
    using SecretHistories.Enums;

    public class StartSituationAction : ActionBase
    {
        public StartSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        public string SituationId { get; }

        protected override Task<bool> OnExecute(CancellationToken cancellationToken)
        {
            var situation = this.GetSituation();

            if (situation.State.Identifier != StateEnum.Unstarted)
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} is already started.");
            }

            situation.TryStart();
            if (situation.State.Identifier == StateEnum.Unstarted)
            {
                throw new ActionFailureException(this, $"Failed to start situation {this.SituationId} for recipe.");
            }

            GameStateProvider.Invalidate();

            return Task.FromResult(true);
        }

        private Situation GetSituation()
        {
            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} not found.");
            }

            return situation;
        }
    }
}