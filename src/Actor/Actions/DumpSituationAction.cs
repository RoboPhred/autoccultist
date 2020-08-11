namespace Autoccultist.Actor.Actions
{
    class DumpSituationAction : IAutoccultistAction
    {
        public string SituationId { get; private set; }


        public DumpSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }
        public bool CanExecute()
        {
            if (!GameAPI.IsInteractable)
            {
                return false;
            }

            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                return false;
            }
            switch (situation.SituationClock.State)
            {
                case SituationState.Unstarted:
                case SituationState.Complete:
                    return true;
                default:
                    return false;
            }
        }

        public void Execute()
        {
            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                throw new ActionFailureException(this, "Situation is not available.");
            }
            switch (situation.SituationClock.State)
            {
                case SituationState.Unstarted:
                    situation.situationWindow.DumpAllStartingCardsToDesktop();
                    break;
                case SituationState.Complete:
                    situation.situationWindow.DumpAllResultingCardsToDesktop();
                    break;
            }
        }
    }
}