namespace Autoccultist.Actor.Actions
{
    /// <summary>
    /// An action to dump all cards out of a situation window.
    /// Supports unstarted and completed situations.
    /// </summary>
    public class DumpSituationAction : IAutoccultistAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DumpSituationAction"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to dump.</param>
        public DumpSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        /// <summary>
        /// Gets the situation id of the situation this action is targeting.
        /// </summary>
        public string SituationId { get; }

        /// <inheritdoc/>
        public void Execute()
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

            switch (situation.SituationClock.State)
            {
                case SituationState.Unstarted:
                    situation.situationWindow.DumpAllStartingCardsToDesktop();
                    break;
                case SituationState.Complete:
                    situation.situationWindow.DumpAllResultingCardsToDesktop();
                    break;
                default:
                    throw new ActionFailureException(this, "Situation is not in a state that can be dumped.");
            }
        }
    }
}
