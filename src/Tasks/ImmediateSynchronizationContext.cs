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
        /// <summary>
        /// The maximum number of actions we expect to take per frame.
        /// If we exceed this, kill the game and log an exception.
        /// </summary>
        /// <remarks>
        /// This is to guard against runaway infinite loops in our async code.
        /// </remarks>
        private const int MaxExpectedActionsPerFrame = 500;

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

                // Queue up this action as a pending action.
                // Note: There might be actions in here that were queued up outside of Run, they will run first.
                instance.pendingActions.Enqueue(((SendOrPostCallback)((s) => action()), (object)null));

                // Drain the queue until we run out of actions to do.
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
            if (this.hardLockGuard > MaxExpectedActionsPerFrame)
            {
                NoonUtility.LogException(new Exception("ImmediateSynchronizationContext hard lock detected."));
            }

            d(state);
        }
    }
}
