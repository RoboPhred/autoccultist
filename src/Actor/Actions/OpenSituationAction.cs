namespace Autoccultist.Actor.Actions
{
    class OpenSituationAction : IAutoccultistAction
    {
        public string SituationId { get; private set; }

        public OpenSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        public void Execute()
        {
            if (!GameAPI.IsInteractable)
            {
                return;
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