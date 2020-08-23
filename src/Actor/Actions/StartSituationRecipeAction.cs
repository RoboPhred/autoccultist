namespace Autoccultist.Actor.Actions
{
    /// <summary>
    /// An action to start a situation processing cards.
    /// </summary>
    public class StartSituationRecipeAction : IAutoccultistAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartSituationRecipeAction"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to start.</param>
        public StartSituationRecipeAction(string situationId)
        {
            this.SituationId = situationId;
        }

        /// <summary>
        /// Gets the situation id of the situation to start.
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

            situation.AttemptActivateRecipe();
            if (situation.SituationClock.State == SituationState.Unstarted)
            {
                throw new ActionFailureException(this, "Failed to start situation.");
            }
        }
    }
}
