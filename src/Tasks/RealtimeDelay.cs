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

        public static async Task<T> Timeout<T>(Func<CancellationToken, Task<T>> func, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var task = func(cts.Token);
            var timeoutTask = Of(timeout, cts.Token);

            var completedTask = await Task.WhenAny(task, timeoutTask);

            // Cancel whatever task didn't complete.
            cts.Cancel();

            if (completedTask == task)
            {
                return await task;
            }
            else
            {
                throw new TimeoutException();
            }
        }

        public static async Task Timeout(Func<CancellationToken, Task> func, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var task = func(cts.Token);
            var timeoutTask = Of(timeout, cts.Token);

            var completedTask = await Task.WhenAny(task, timeoutTask);

            // Cancel whatever task didn't complete.
            cts.Cancel();

            if (completedTask == task)
            {
                await task;
            }
            else
            {
                throw new TimeoutException();
            }
        }
    }
}
