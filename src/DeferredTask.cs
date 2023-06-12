namespace AutoccultistNS
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class DeferredTask<T>
    {
        private readonly TaskCompletionSource<T> source = new TaskCompletionSource<T>();

        private readonly CancellationTokenSource selfCancel = new();
        private readonly CancellationToken cancellationToken;

        private Func<CancellationToken, Task<T>> taskSource;
        private Task<T> innerTask;

        public DeferredTask(Func<CancellationToken, Task<T>> taskSource, CancellationToken cancellationToken)
        {
            this.taskSource = taskSource;
            this.cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.selfCancel.Token).Token;
            cancellationToken.Register(() => this.Cancel());
        }

        public Task<T> Task => this.source.Task;

        public void Cancel()
        {
            this.selfCancel.Cancel();

            // Only cancel the source if we have not yet executed.
            // If we have started executing, let the cancellation propogate.
            if (this.taskSource == null)
            {
                this.source.SetCanceled();
            }
        }

        public async Task<T> Execute()
        {
            if (this.innerTask != null)
            {
                throw new InvalidOperationException("Task has already been executed.");
            }

            this.innerTask = this.taskSource(this.cancellationToken);

            try
            {
                T result = await this.innerTask;
                this.source.SetResult(result);
                return result;
            }
            catch (TaskCanceledException)
            {
                this.source.SetCanceled();
                throw;
            }
            catch (Exception ex)
            {
                this.source.SetException(ex);
                throw;
            }
        }
    }
}
