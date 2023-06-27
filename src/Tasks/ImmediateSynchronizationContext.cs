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
            try
            {
                SetSynchronizationContext(instance);

                (SendOrPostCallback, object) pending;

                // Drain any pending actions that got scheduled after we last finished running
                while ((pending = instance.pendingActions.DequeueOrDefault()).Item1 != null)
                {
                    pending.Item1(pending.Item2);
                }

                action();

                // Drain all pending actions that got scheduled by our action.
                while ((pending = instance.pendingActions.DequeueOrDefault()).Item1 != null)
                {
                    pending.Item1(pending.Item2);
                }
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
                d(state);
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
    }
}
