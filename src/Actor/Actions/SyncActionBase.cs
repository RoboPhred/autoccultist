namespace AutoccultistNS.Actor.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class SyncActionBase : IAutoccultistAction
    {
        private bool executed = false;

        public Task<ActionResult> Execute(CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<ActionResult>();

            if (this.executed)
            {
                source.SetException(new ActionFailureException(this, "Action has already executed."));
                return source.Task;
            }

            this.executed = true;
            try
            {
                source.SetResult(this.OnExecute());
            }
            catch (Exception ex)
            {
                source.SetException(ex);
            }

            return source.Task;
        }

        protected abstract ActionResult OnExecute();
    }
}
