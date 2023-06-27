namespace AutoccultistNS
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Tasks;

    public static class RealtimeDelay
    {
        public static Task Of(TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            return Of(timeSpan.TotalMilliseconds, cancellationToken);
        }

        public static Task Of(double milliseconds, CancellationToken cancellationToken)
        {
            var then = DateTime.Now + TimeSpan.FromMilliseconds(milliseconds);
            return AwaitConditionTask.From(() => DateTime.Now >= then, cancellationToken);
        }

        public static Task Timeout(Func<CancellationToken, Task> func, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var task = func(cts.Token);

            // Cancel token on task completion, so the timeout doesnt continue.
            task.ContinueWith(t => cts.Cancel());

            var timeoutTask = Of(timeout, cts.Token);

            // Cancel the token on timeout, so the origin task doesn't continue.
            timeoutTask = timeoutTask.ContinueWith(t =>
            {
                cts.Cancel();
                throw new TimeoutException();
            });

            return Task.WhenAny(task, timeoutTask);
        }
    }
}
