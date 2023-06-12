namespace AutoccultistNS.Actor.Actions
{
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class ActionBase : IAutoccultistAction
    {
        private bool executed = false;

        public Task<ActionResult> Execute(CancellationToken cancellationToken)
        {
            if (this.executed)
            {
                throw new ActionFailureException(this, "Action has already executed.");
            }

            this.executed = true;
            return this.OnExecute(cancellationToken);
        }

        protected abstract Task<ActionResult> OnExecute(CancellationToken cancellationToken);
    }
}
