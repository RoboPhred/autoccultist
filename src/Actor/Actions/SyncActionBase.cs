namespace AutoccultistNS.Actor.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class SyncActionBase : IAutoccultistAction
    {
        private bool executed = false;

        public Task<bool> Execute(CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<bool>();

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

        protected abstract bool OnExecute();
    }
}
