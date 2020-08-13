namespace Autoccultist.Actor.Actions
{
    class SetPausedAction : IAutoccultistAction
    {
        public bool Paused { get; private set; }


        public SetPausedAction(bool paused)
        {
            this.Paused = paused;
        }

        public void Execute()
        {
            if (!GameAPI.IsInteractable)
            {
                return;
            }
            GameAPI.SetPaused(Paused);
        }
    }
}