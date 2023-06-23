namespace AutoccultistNS.Actor.Actions
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Tasks;
    using SecretHistories.Enums;

    /// <summary>
    /// An action to dump all cards out of a situation window.
    /// Supports unstarted and completed situations.
    /// </summary>
    public class ConcludeSituationAction : ActionBase
    {
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

            if (situation.State.Identifier == StateEnum.Unstarted || situation.State.Identifier == StateEnum.RequiringExecution)
            {
                var thresholdTokens = situation.GetCurrentThresholdSpheres().SelectMany(s => s.GetTokens()).ToArray();
                if (thresholdTokens.Length == 0)
                {
                    return false;
                }

                if (!situation.IsOpen)
                {
                    situation.OpenAt(situation.Token.Location);
                    await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
                }

                situation.DumpUnstartedBusiness();

                // Wait for the tokens to finish moving.
                // We used to check if they ended up on the table, but some tokens can be yoinked by greedy slots enroute.
                var awaitSphereFilled = AwaitConditionTask.From(() => thresholdTokens.TokensAreStable(), cancellationToken);
                if (await Task.WhenAny(awaitSphereFilled, UnityDelay.Of(1000, cancellationToken)) != awaitSphereFilled)
                {
                    throw new ActionFailureException(this, $"Timed out waiting for threshold cards to stabilize from the dumping of situation {this.SituationId}.");
                }
            }
            else if (situation.State.Identifier == StateEnum.Complete)
            {
                if (!situation.IsOpen)
                {
                    situation.OpenAt(situation.Token.Location);
                    await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
                }

                var outputTokens = situation.GetSpheresByCategory(SphereCategory.Output).SelectMany(s => s.GetTokens()).ToList();

                if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
                {
                    var shroudedTokens = outputTokens.Where(x => x.Shrouded).ToArray();
                    foreach (var token in shroudedTokens)
                    {
                        token.Unshroud();
                    }

                    if (shroudedTokens.Length > 0)
                    {
                        await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
                    }
                }

                situation.Conclude();

                // Wait for the tokens to finish moving.
                // We used to check if they ended up on the table, but some tokens can be yoinked by greedy slots enroute.
                var awaitSphereFilled = AwaitConditionTask.From(() => outputTokens.TokensAreStable(), cancellationToken);
                if (await Task.WhenAny(awaitSphereFilled, UnityDelay.Of(1000, cancellationToken)) != awaitSphereFilled)
                {
                    throw new ActionFailureException(this, $"Timed out waiting for output cards to stabilize from the conclusion of situation {this.SituationId}.");
                }
            }
            else
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} cannot be emptied becuase it is in state {situation.State.Identifier}.");
            }

            // Situation might go away when we Conclude
            if (!situation.Token.Defunct && situation.IsOpen)
            {
                // Wait on a heartbeat but dont use any delay, as we waited on the cards
                await MechanicalHeart.AwaitBeat(cancellationToken, TimeSpan.Zero);

                // NOTE: Game crash here... probably on temporary / dissipating situations.
                // The above doesn't seem enough to gate it.  Let's check again.
                // Note: This is really weird, as AwaitBeat with TimeSpan.Zero should no-op and we shouldn't see any changes?
                if (situation.Token.Defunct || !situation.IsOpen)
                {
                    Autoccultist.LogWarn($"Situation {this.SituationId} was closed while we were concluding it.  This is probably fine, but it means something is wonky with threading.");
                }
                else
                {
                    situation.Close();
                }
            }

            return true;
        }
    }
}
