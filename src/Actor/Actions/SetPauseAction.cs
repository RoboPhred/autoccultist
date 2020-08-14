namespace Autoccultist.Actor.Actions
{
    class SetPausedAction : IAutoccultistAction
    {
        public bool Paused { get; private set; }


        public SetPausedAction(bool paused)
        {
            this.Paused = paused;
        }
        public bool CanExecute()
        {
            return GameAPI.IsInteractable;
        }

        public void Execute()
        {
            if (!GameAPI.IsInteractable)
            {
                throw new ActionFailureException(this, "Game is not interactable at this moment.");
            }

            GameAPI.SetPaused(Paused);
        }
    }
}