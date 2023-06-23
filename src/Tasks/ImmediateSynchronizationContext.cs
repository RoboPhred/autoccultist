namespace AutoccultistNS
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// A SynchronizationContext that executes all actions on the same frame if possible.
    /// </summary>
    public class ImmediateSynchronizationContext : SynchronizationContext
    {
        private static ImmediateSynchronizationContext instance;

        private readonly int threadId = Thread.CurrentThread.ManagedThreadId;
        private readonly Queue<(SendOrPostCallback, object)> pendingActions = new();
        private readonly SynchronizationContext fallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmediateSynchronizationContext"/> class.
        /// </summary>
        private ImmediateSynchronizationContext(SynchronizationContext fallback)
        {
            this.fallback = fallback;
        }

        /// <summary>
        /// Runs the given action on the current thread, and complete all posted actions before returning.
        /// Actions scheduled by continuations outside of this function will be ran when this function is next called.
        /// </summary>
        public static void Run(Action action)
        {
            if (instance == null)
            {
                instance = new ImmediateSynchronizationContext(Current);
            }

            if (Current == instance)
            {
                action();
                return;
            }

            var oldContext = Current;
            try
            {
                SetSynchronizationContext(instance);

                (SendOrPostCallback, object) pending;

                // Drain any pending actions that got scheduled after we last finished running
                int count = 0;
                while ((pending = instance.pendingActions.DequeueOrDefault()).Item1 != null)
                {
                    // We might queue up actions doing this, so count the actual number.
                    count++;

                    pending.Item1(pending.Item2);
                }

                if (count > 0)
                {
                    NoonUtility.LogWarning($"Drained {count} pending actions before running current action.");
                }

                action();

                // Drain all pending actions that got scheduled by our action.]
                count = 0;
                while ((pending = instance.pendingActions.DequeueOrDefault()).Item1 != null)
                {
                    count++;
                    pending.Item1(pending.Item2);
                }

                if (count > 0)
                {
                    NoonUtility.LogWarning($"Drained {count} followup actions after running current action.");
                }
            }
            finally
            {
                SetSynchronizationContext(oldContext);
            }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (Thread.CurrentThread.ManagedThreadId == this.threadId)
            {
                d(state);
                return;
            }

            NoonUtility.LogWarning($"Sending action to thread {Thread.CurrentThread.ManagedThreadId} from thread {this.threadId}.");
            this.fallback.Send(d, state);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            if (Thread.CurrentThread.ManagedThreadId == this.threadId)
            {
                d(state);
                return;
            }

            NoonUtility.LogWarning($"Posting action to thread {Thread.CurrentThread.ManagedThreadId} from thread {this.threadId}.");

            this.fallback.Post(d, state);
        }
    }
}
