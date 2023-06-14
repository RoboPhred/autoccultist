namespace AutoccultistNS.Actor.Actions
{
    /// <summary>
    /// An action that closes a situation window.
    /// </summary>
    public class CloseSituationAction : SyncActionBase
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

        public override string ToString()
        {
            return $"CloseSituationAction(SituationId = {this.SituationId})";
        }

        /// <inheritdoc/>
        protected override bool OnExecute()
        {
            if (GameAPI.IsInMansus)
            {
                return false;
            }

            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null || !situation.IsOpen)
            {
                return false;
            }

            situation.Close();
            return true;
        }
    }
}
