namespace AutoccultistNS.Actor.Actions
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Tasks;
    using SecretHistories.Enums;

    /// <summary>
    /// An action to dump all cards out of a situation window.
    /// Supports unstarted and completed situations.
    /// </summary>
    public class ConcludeSituationAction : ActionBase
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcludeSituationAction"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to dump.</param>
        /// <param name="useWindow">Whether to use the situation window to dump the situation.</param>
        public ConcludeSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        /// <summary>
        /// Gets the situation id of the situation this action is targeting.
        /// </summary>
        public string SituationId { get; }

        public override string ToString()
        {
            return $"ConcludeSituationAction(SituationId = {this.SituationId})";
        }

        /// <inheritdoc/>
        protected override async Task<bool> OnExecute(CancellationToken cancellationToken)
        {
            if (GameAPI.IsInMansus)
            {
                throw new ActionFailureException(this, "Cannot interact with situations when in the mansus.");
            }

            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} is not available.");
            }

            if (situation.State.Identifier != StateEnum.Complete)
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} is not complete.");
            }

            var outputTokens = situation.GetSpheresByCategory(SphereCategory.Output).SelectMany(s => s.GetTokens()).ToList();

            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
            {
                var shroudedTokens = outputTokens.Where(x => x.Payload.IsShrouded).ToArray();
                if (shroudedTokens.Length > 0)
                {
                    foreach (var token in shroudedTokens)
                    {
                        token.Payload.Unshroud();
                    }

                    await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
                }
            }

            situation.Conclude();

            // Wait for the tokens to finish moving.
            // We used to check if they ended up on the table, but some tokens can be yoinked by greedy slots enroute.
            try
            {
                await RealtimeDelay.Timeout(c => AwaitConditionTask.From(() => outputTokens.TokensAreStable(), c), Timeout, cancellationToken);
            }
            catch (TimeoutException)
            {
                throw new ActionFailureException(this, $"Timed out waiting for output cards to stabilize from the conclusion of situation {this.SituationId}.");
            }

            return true;
        }
    }
}
