namespace AutoccultistNS.Tasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class GlobalUpdateTask<T> : IDisposable
    {
        private readonly TaskCompletionSource<T> taskCompletionSource = new();
        private readonly CancellationToken cancellationToken;

        private bool isDisposed = false;

        protected GlobalUpdateTask(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            Autoccultist.GlobalUpdate += this.OnUpdate;
        }

        ~GlobalUpdateTask()
        {
            this.Dispose();
        }

        public Task<T> Task => this.taskCompletionSource.Task;

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            GC.SuppressFinalize(this);

            this.taskCompletionSource.TrySetCanceled();

            Autoccultist.GlobalUpdate -= this.OnUpdate;
        }

        protected abstract void Update();

        protected void SetResult(T value)
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
                throw new ObjectDisposedException(nameof(GlobalUpdateTask<T>));
            }
        }

        private void OnUpdate(object sender, EventArgs e)
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
