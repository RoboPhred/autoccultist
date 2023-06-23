namespace AutoccultistNS
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Tasks;

    public static class UnityDelay
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
    }
}
