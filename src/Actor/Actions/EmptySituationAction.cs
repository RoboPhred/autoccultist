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
                var thresholdTokens = situation.GetSpheresByCategory(SphereCategory.Threshold).SelectMany(s => s.GetTokens()).ToArray();
                if (thresholdTokens.Length == 0)
                {
                    return false;
                }

                if (!situation.IsOpen)
                {
                    situation.OpenAt(situation.Token.Location);
                    await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
                }

                var tabletop = GameAPI.TabletopSphere;

                // Wait for the tokens to hit the table.
                // We do not want to prematurely let our operation end before the tokens are on the table, as this can
                // make lower priority impulses mistakenly trigger as the higher priority ops think we do not have the cards we need.
                // Would be nice if there was a way to subscribe to the itinerary, but whatever...
                // Some cards might disappear during this process, so only compare the ones not defunct.
                var awaitSphereFilled = new AwaitConditionTask(() => tabletop.GetTokens().ContainsAll(thresholdTokens.Where(x => !x.Defunct)), cancellationToken);
                if (await Task.WhenAny(awaitSphereFilled.Task, Task.Delay(1000, cancellationToken)) != awaitSphereFilled.Task)
                {
                    throw new ActionFailureException(this, $"Timed out waiting for threshold cards to travel from situation {this.SituationId} to the tabletop.");
                }
            }
            else if (situation.State.Identifier == StateEnum.Complete)
            {
                if (!situation.IsOpen)
                {
                    situation.OpenAt(situation.Token.Location);
                    await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
                }

                var debugLog = from spheres in situation.GetSpheresByCategory(SphereCategory.Output)
                               from token in spheres.GetTokens()
                               select token.PayloadEntityId;

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

                var tabletop = GameAPI.TabletopSphere;

                // Wait for the tokens to hit the table.
                // We do not want to prematurely let our operation end before the tokens are on the table, as this can
                // make lower priority impulses mistakenly trigger as the higher priority ops think we do not have the cards we need.
                // Would be nice if there was a way to subscribe to the itinerary, but whatever...
                // Some cards might disappear during this process, so only compare the ones not defunct.
                var awaitSphereFilled = new AwaitConditionTask(() => tabletop.GetTokens().ContainsAll(outputTokens.Where(x => !x.Defunct)), cancellationToken);
                if (await Task.WhenAny(awaitSphereFilled.Task, Task.Delay(1000, cancellationToken)) != awaitSphereFilled.Task)
                {
                    throw new ActionFailureException(this, $"Timed out waiting for output cards to travel from situation {this.SituationId} to the tabletop.");
                }

                // Note: It takes a while for the cards to animate onto the table.
                // It would be nice if we could include that time in our next event time, so we don't wait so long to close the situation.
                // or even better, close it after a heartbeat then wait for the cards to finish moving.
                // We might want to combine this with CloseSituationAction into a full ConcludeSituationAction

                Autoccultist.Instance.LogTrace($"Situation {this.SituationId} concluded with cards {string.Join(", ", debugLog)}.");

                // Wait on a heartbeat but dont use any delay, as we waited on the cards
                await MechanicalHeart.AwaitBeat(cancellationToken, TimeSpan.Zero);
            }
            else
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} cannot be emptied becuase it is in state {situation.State.Identifier}.");
            }

            // Situation might go away when we Conclude
            if (!situation.Token.Defunct && situation.IsOpen)
            {
                situation.Close();
            }

            GameStateProvider.Invalidate();
            return true;
        }
    }
}
