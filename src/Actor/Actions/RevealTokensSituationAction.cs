namespace AutoccultistNS.Actor.Actions
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SecretHistories.Entities;
    using SecretHistories.Enums;

    public class RevealTokensSituationAction : ActionBase
    {
        public RevealTokensSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        public string SituationId { get; }

        protected override async Task<bool> OnExecute(CancellationToken cancellationToken)
        {
            var situation = this.GetSituation();

            if (situation.State.Identifier != StateEnum.Complete)
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} is not complete.");
            }

            var outputTokens = situation.GetSpheresByCategory(SphereCategory.Output).SelectMany(s => s.GetTokens()).ToList();

            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
            {
                var shroudedTokens = outputTokens.Where(x => x.Shrouded).ToArray();
                if (shroudedTokens.Length > 0)
                {
                    foreach (var token in shroudedTokens)
                    {
                        token.Unshroud();
                    }

                    await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
                    return true;
                }
            }

            return false;
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
