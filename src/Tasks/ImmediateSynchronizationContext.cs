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

        private int hardLockGuard = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmediateSynchronizationContext"/> class.
        /// </summary>
        private ImmediateSynchronizationContext()
        {
        }

        /// <summary>
        /// Runs the given action on the current thread, and complete all posted actions before returning.
        /// Actions scheduled by continuations outside of this function will be ran when this function is next called.
        /// </summary>
        public static void Run(Action action)
        {
            if (instance == null)
            {
                instance = new ImmediateSynchronizationContext();
            }

            if (Current == instance)
            {
                action();
                return;
            }

            var lastContext = Current;
            instance.hardLockGuard = 0;
            try
            {
                SetSynchronizationContext(instance);

                // Drain any pending actions that got scheduled after we last finished running
                instance.DrainQueue();

                instance.hardLockGuard = 0;
                action();

                // Drain all pending actions that got scheduled by our action.
                instance.DrainQueue();
            }
            finally
            {
                SetSynchronizationContext(lastContext);
                lastContext = null;
            }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (Thread.CurrentThread.ManagedThreadId == this.threadId)
            {
                this.HandleCallback(d, state);
                return;
            }

            Autoccultist.LogWarn($"Sending action to thread {Thread.CurrentThread.ManagedThreadId} from thread {this.threadId}.");
            this.pendingActions.Enqueue((d, state));
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            if (Thread.CurrentThread.ManagedThreadId != this.threadId)
            {
                Autoccultist.LogWarn($"Posting action to thread {Thread.CurrentThread.ManagedThreadId} from thread {this.threadId}.");
            }

            this.pendingActions.Enqueue((d, state));
        }

        private void DrainQueue()
        {
            this.hardLockGuard = 0;

            (SendOrPostCallback, object) pending;

            while ((pending = instance.pendingActions.DequeueOrDefault()).Item1 != null)
            {
                this.HandleCallback(pending.Item1, pending.Item2);
            }
        }

        private void HandleCallback(SendOrPostCallback d, object state)
        {
            this.hardLockGuard++;
            if (this.hardLockGuard > 1000)
            {
                NoonUtility.LogException(new Exception("ImmediateSynchronizationContext hard lock detected."));
            }

            d(state);
        }
    }
}
