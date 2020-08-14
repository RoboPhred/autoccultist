namespace Autoccultist.Actor.Actions
{
    class CloseSituationAction : IAutoccultistAction
    {
        public string SituationId { get; private set; }

        public bool IgnoreFailures { get; set; }


        public CloseSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        public void Execute()
        {
            if (!GameAPI.IsInteractable)
            {
                if (this.IgnoreFailures)
                {
                    return;
                }
                throw new ActionFailureException(this, "Game is not interactable at this moment.");
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