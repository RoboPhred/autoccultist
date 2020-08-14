namespace Autoccultist.Actor.Actions
{
    class DumpSituationAction : IAutoccultistAction
    {
        public string SituationId { get; private set; }


        public DumpSituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        public void Execute()
        {
            if (!GameAPI.IsInteractable)
            {
                throw new ActionFailureException(this, "Game is not interactable at this moment.");
            }


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
                default:
                    throw new ActionFailureException(this, "Situation is not in a state that can be dumped.");
            }
        }
    }
}