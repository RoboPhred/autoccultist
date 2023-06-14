namespace AutoccultistNS.Actor.Actions
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.GameState;
    using SecretHistories.Enums;
    using SecretHistories.UI;

    /// <summary>
    /// An action to dump all cards out of a situation window.
    /// Supports unstarted and completed situations.
    /// </summary>
    public class EmptySituationAction : ActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptySituationAction"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to dump.</param>
        public EmptySituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        /// <summary>
        /// Gets the situation id of the situation this action is targeting.
        /// </summary>
        public string SituationId { get; }

        public override string ToString()
        {
            return $"EmptySituationAction(SituationId = {this.SituationId})";
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
                throw new ActionFailureException(this, "Situation is not available.");
            }

            if (situation.State.Identifier == StateEnum.Unstarted)
            {
                if (!situation.GetSpheresByCategory(SphereCategory.Threshold).SelectMany(s => s.GetTokens()).Any())
                {
                    return false;
                }

                situation.DumpUnstartedBusiness();
            }
            else if (situation.State.Identifier == StateEnum.Complete)
            {
                var debugLog = from spheres in situation.GetSpheresByCategory(SphereCategory.Output)
                               from token in spheres.GetTokens()
                               select token.PayloadEntityId;

                if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                {
                    var shroudedTokens = situation.GetSpheresByCategory(SphereCategory.Output).SelectMany(s => s.GetTokens()).Where(x => x.Shrouded).ToList();
                    foreach (var token in shroudedTokens)
                    {
                        token.Unshroud();
                        await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
                    }
                }

                situation.Conclude();

                Autoccultist.Instance.LogTrace($"Situation {this.SituationId} concluded with cards {string.Join(", ", debugLog)}.");
            }
            else
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} cannot be emptied becuase it is in state {situation.State.Identifier}.");
            }

            GameStateProvider.Invalidate();
            return true;
        }
    }
}
