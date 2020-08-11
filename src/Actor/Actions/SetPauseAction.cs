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
            GameAPI.SetPaused(Paused);
        }
    }
}