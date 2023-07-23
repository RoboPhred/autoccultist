namespace AutoccultistNS.Actor.Actions
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Tasks;
    using SecretHistories.Entities;
    using SecretHistories.Enums;

    public class DumpUnstartedSituationAction : ActionBase
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

        public DumpUnstartedSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        public string SituationId { get; }

        protected override async Task<bool> OnExecute(CancellationToken cancellationToken)
        {
            var situation = this.GetSituation();

            if (situation.State.Identifier != SecretHistories.Enums.StateEnum.Unstarted)
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} is not unstarted.");
            }

            var tokens = situation.GetSpheresByCategory(SphereCategory.Threshold).SelectMany(s => s.GetTokens()).ToList();

            if (tokens.Count == 0)
            {
                return false;
            }

            situation.DumpUnstartedBusiness();

            // Wait for the tokens to finish moving.
            // We used to check if they ended up on the table, but some tokens can be yoinked by greedy slots enroute.
            try
            {
                await RealtimeDelay.Timeout(c => AwaitConditionTask.From(() => tokens.TokensAreStable(), c), Timeout, cancellationToken);
            }
            catch (TimeoutException)
            {
                throw new ActionFailureException(this, $"Timed out waiting for output cards to stabilize from the conclusion of situation {this.SituationId}.");
            }

            return true;
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
