namespace AutoccultistNS.Actor.Actions
{
    using AutoccultistNS.GameState;

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
        public ConcludeSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        /// <summary>
        /// Gets the situation id of the situation this action is targeting.
        /// </summary>
        public string SituationId { get; }

        /// <inheritdoc/>
        public override void Execute()
        {
            this.VerifyNotExecuted();

            if (GameAPI.IsInMansus)
            {
                throw new ActionFailureException(this, "Cannot interact with situations when in the mansus.");
            }

            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                throw new ActionFailureException(this, "Situation is not available.");
            }

            // This used to just dump the contents, but this Conclude function seems to be used by the UI to accept everything.
            // Note that this will cause temporary situations to retire, which I think was happening anyway with the previous code.
            situation.Conclude();
            GameStateProvider.Invalidate();
        }
    }
}
