namespace AutoccultistNS.Actor.Actions
{
    /// <summary>
    /// An action to open a situation window.
    /// </summary>
    public class OpenSituationAction : ActionBase
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

        public override string ToString()
        {
            return $"OpenSituationAction(SituationId = {this.SituationId})";
        }

        /// <inheritdoc/>
        protected override ActionResult OnExecute()
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

            if (situation.IsOpen)
            {
                return ActionResult.NoOp;
            }

            situation.OpenAt(situation.Token.Location);
            return ActionResult.Completed;
        }
    }
}
