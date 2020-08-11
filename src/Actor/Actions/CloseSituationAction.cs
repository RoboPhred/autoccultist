namespace Autoccultist.Actor.Actions
{
    class CloseSituationAction : IAutoccultistAction
    {
        public string SituationId { get; private set; }


        public CloseSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }
        public bool CanExecute()
        {
            return GameAPI.IsInteractable && GameAPI.GetSituation(this.SituationId) != null;
        }

        public void Execute()
        {
            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                throw new ActionFailureException(this, "Situation is not available.");
            }

            situation.CloseWindow();
        }
    }
}