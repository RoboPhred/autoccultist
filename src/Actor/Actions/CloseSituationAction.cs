namespace Autoccultist.Actor.Actions
{
    /// <summary>
    /// An action that closes a situation window.
    /// </summary>
    public class CloseSituationAction : IAutoccultistAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloseSituationAction"/> class.
        /// </summary>
        /// <param name="situationId">The id of the situation to close.</param>
        public CloseSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        /// <summary>
        /// Gets the situation id of the situation to close the window for.
        /// </summary>
        public string SituationId { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this action should ignore failures.
        /// This can be useful for cases when the situation may dissapear after being interacted with.
        /// </summary>
        public bool IgnoreFailures { get; set; }

        /// <inheritdoc/>
        public void Execute()
        {
            if (GameAPI.IsInMansus)
            {
                if (this.IgnoreFailures)
                {
                    return;
                }

                throw new ActionFailureException(this, "Cannot interact with situations when in the mansus.");
            }

            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                if (this.IgnoreFailures)
                {
                    return;
                }

                throw new ActionFailureException(this, "Situation is not available.");
            }

            situation.CloseWindow();
        }
    }
}
