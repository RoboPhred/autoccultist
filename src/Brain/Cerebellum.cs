namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Tasks;

    /// <summary>
    /// The actor, that performs game actions on our behalf.
    /// </summary>
    public static class Cerebellum
    {
        private static readonly Queue<DeferredTask<object>> PendingActions = new();
        private static DeferredTask<object> currentAction;

        private static bool isDrainingQueue = false;

        /// <summary>
        /// Queue up an asynchronous action for fifo execution.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">An optional CancellationToken used to cancel the action execution.</param>
        /// <returns>A task that will complete when all actions complete, or fail with an exception describing the action failure.</returns>
        public static Task Coordinate(Func<CancellationToken, Task> action, CancellationToken? cancellationToken = null)
        {
            var pendingAction = new DeferredTask<object>(
                (innerCancellationToken) => action(innerCancellationToken).ContinueWith(_ => (object)null),
                cancellationToken ?? CancellationToken.None);

            PendingActions.Enqueue(pendingAction);

            if (!isDrainingQueue)
            {
                DrainActions();
            }

            return pendingAction.Task;
        }

        public static Task<T> Coordinate<T>(Func<CancellationToken, Task<T>> func, CancellationToken? cancellationToken = null)
        {
            // Need to box our result, no matter what it is.
            var pendingAction = new DeferredTask<object>((innerCancellationToken) => func(innerCancellationToken).ContinueWith(x => (object)x.Result), cancellationToken ?? CancellationToken.None);

            PendingActions.Enqueue(pendingAction);

            if (!isDrainingQueue)
            {
                DrainActions();
            }

            // Now we need to unbox it.  Sigh...
            return pendingAction.Task.ContinueWith(x => (T)x.Result);
        }

        /// <summary>
        /// Abort all ongoing actions.
        /// </summary>
        public static void AbortAllActions()
        {
            currentAction?.Cancel();
            currentAction = null;

            DeferredTask<object> queuedTask;
            while ((queuedTask = PendingActions.DequeueOrDefault()) != null)
            {
                queuedTask.Cancel();
            }
        }

        public static async void DrainActions()
        {
            if (isDrainingQueue || PendingActions.Count == 0)
            {
                return;
            }

            isDrainingQueue = true;
            try
            {
                OnActive();

                DeferredTask<object> set;
                while ((set = PendingActions.DequeueOrDefault()) != null)
                {
                    await MechanicalHeart.AwaitBeat(CancellationToken.None, TimeSpan.Zero);

                    currentAction = set;
                    try
                    {
                        await set.Execute();
                    }
                    catch (Exception ex)
                    {
                        Autoccultist.LogWarn("Failed to perform action: " + ex.ToString());
                    }
                    finally
                    {
                        currentAction = null;
                    }

                    // AutoccultistActor used to wait an action delay here, but now we are being used to linearify tasks that are not specifically game actions.
                }
            }
            finally
            {
                currentAction = null;
                isDrainingQueue = false;
                OnIdle();
            }
        }

        private static void OnActive()
        {
        }

        private static void OnIdle()
        {
        }
    }
}
