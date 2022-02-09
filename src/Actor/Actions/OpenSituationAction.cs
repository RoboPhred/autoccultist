namespace Autoccultist.Actor.Actions
{
    /// <summary>
    /// An action to open a situation window.
    /// </summary>
    public class OpenSituationAction : IAutoccultistAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSituationAction"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to open the window.</param>
        public OpenSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        /// <summary>
        /// Gets the situation id of the situation to open the window for.
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

            situation.OpenWindow();
        }
    }
}
