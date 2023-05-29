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

        /// <inheritdoc/>
        public override void Execute()
        {
            this.VerifyNotExecuted();

            if (GameAPI.IsInMansus)
            {
                return;
            }

            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                return;
            }

            situation.OpenAt(situation.Token.Location);
        }
    }
}
