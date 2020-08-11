namespace Autoccultist.Actor.Actions
{
    class OpenSituationAction : IAutoccultistAction
    {
        public string SituationId { get; private set; }


        public OpenSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }
        public bool CanExecute()
        {
            if (!GameAPI.IsInteractable)
            {
                return false;
            }

            return GameAPI.GetSituation(this.SituationId) != null;
        }

        public void Execute()
        {
            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                throw new ActionFailureException(this, "Situation is not available.");
            }

            situation.OpenWindow();
        }
    }
}