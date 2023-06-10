namespace AutoccultistNS.Actor.Actions
{
    public abstract class ActionBase : IAutoccultistAction
    {
        private bool executed = false;

        public ActionResult Execute()
        {
            if (this.executed)
            {
                throw new ActionFailureException(this, "Action has already executed.");
            }

            this.executed = true;
            return this.OnExecute();
        }

        protected abstract ActionResult OnExecute();
    }
}
