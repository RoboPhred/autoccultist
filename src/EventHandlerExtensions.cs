namespace AutoccultistNS
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class EventHandlerExtensions
    {
        // Turns out EventHandlers are byval...
        // public static Task<T> AwaitEvent<T>(this EventHandler<T> eventHandler, CancellationToken? cancellationToken = null)
        // {
        //     var awaiter = new EventAwaiter<T>(eventHandler, cancellationToken ?? CancellationToken.None);
        //     return awaiter.Task;
        // }
        public static Task<T> AwaitEvent<T>(Action<EventHandler<T>> subscribe, Action<EventHandler<T>> unsubscribe, CancellationToken? cancellationToken = null)
        {
            var awaiter = new EventAwaiter<T>(subscribe, unsubscribe, cancellationToken ?? CancellationToken.None);
            return awaiter.Task;
        }

        private class EventAwaiter<T>
        {
            private readonly TaskCompletionSource<T> tcs = new();
            private readonly Action<EventHandler<T>> unsubscribe;

            public EventAwaiter(Action<EventHandler<T>> subscribe, Action<EventHandler<T>> unsubscribe, CancellationToken cancellationToken)
            {
                subscribe(this.Handler);
                cancellationToken.Register(() =>
                {
                    unsubscribe(this.Handler);
                    this.tcs.TrySetCanceled();
                });
                this.unsubscribe = unsubscribe;
            }

            public Task<T> Task => this.tcs.Task;

            private void Handler(object sender, T eventArgs)
            {
                this.unsubscribe(this.Handler);
                this.tcs.TrySetResult(eventArgs);
            }
        }
    }
}
