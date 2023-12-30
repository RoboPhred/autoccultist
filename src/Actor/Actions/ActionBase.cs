namespace AutoccultistNS.Actor.Actions
{
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class ActionBase : IAutoccultistAction
    {
        private bool executed = false;

        public Task<bool> Execute(CancellationToken cancellationToken)
        {
            if (this.executed)
            {
                return Task.FromException<bool>(new ActionFailureException(this, "Action has already executed."));
            }

            this.executed = true;
            return this.OnExecute(cancellationToken);
        }

        protected abstract Task<bool> OnExecute(CancellationToken cancellationToken);
    }
}
