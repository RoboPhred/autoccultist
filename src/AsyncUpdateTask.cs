namespace AutoccultistNS
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class AsyncUpdateTask<T> : IDisposable
    {
        private readonly TaskCompletionSource<T> taskCompletionSource = new();
        private readonly CancellationToken cancellationToken;

        private bool isDisposed = false;

        protected AsyncUpdateTask(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            MechanicalHeart.OnBeat += this.OnBeat;
        }

        ~AsyncUpdateTask()
        {
            this.Dispose();
        }

        public Task AwaitCompletion()
        {
            return this.taskCompletionSource.Task;
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            GC.SuppressFinalize(this);

            this.taskCompletionSource.TrySetCanceled();

            MechanicalHeart.OnBeat -= this.OnBeat;
        }

        protected abstract void Update();

        protected void SetComplete(T value)
        {
            this.EnsureNotDisposed();

            this.taskCompletionSource.TrySetResult(value);
            this.Dispose();
        }

        protected void SetCanceled()
        {
            this.EnsureNotDisposed();

            this.taskCompletionSource.TrySetCanceled();
            this.Dispose();
        }

        protected void SetException(Exception exception)
        {
            this.EnsureNotDisposed();

            this.taskCompletionSource.TrySetException(exception);
            this.Dispose();
        }

        protected void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(AsyncUpdateTask<T>));
            }
        }

        private void OnBeat(object sender, EventArgs e)
        {
            if (this.cancellationToken.IsCancellationRequested)
            {
                this.SetCanceled();
                return;
            }

            try
            {
                this.Update();
            }
            catch (Exception ex)
            {
                this.SetException(ex);
            }
        }
    }
}
