namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Tasks;

    /// <summary>
    /// The actor, that performs game actions on our behalf.
    /// </summary>
    public static class Cerebellum
    {
        private const bool DebugAllTasks = false;

        private static readonly Queue<DeferredTask<object>> PendingActions = new();
        private static DeferredTask<object> currentAction;

        private static bool isDrainingQueue = false;

        /// <summary>
        /// Queue up an asynchronous action for fifo execution.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">An optional CancellationToken used to cancel the action execution.</param>
        /// <param name="taskName">An optional name for the task.</param>
        /// <returns>A task that will complete when all actions complete, or fail with an exception describing the action failure.</returns>
        public static Task Coordinate(Func<CancellationToken, Task> action, CancellationToken? cancellationToken = null, string taskName = null)
        {
            if (DebugAllTasks && taskName == null)
            {
                taskName = new StackTrace().GetFrame(1).GetMethod().Name;
            }

            var pendingAction = new DeferredTask<object>(
                async (innerCancellationToken) =>
                {
                    await action(innerCancellationToken);
                    return null;
                },
                cancellationToken ?? CancellationToken.None);
            pendingAction.Name = taskName;

            PendingActions.Enqueue(pendingAction);
            NoonUtility.Log($"Queued coordinated task {taskName}");

            if (!isDrainingQueue)
            {
                DrainActions();
            }

            return pendingAction.Task;
        }

        public static Task<T> Coordinate<T>(Func<CancellationToken, Task<T>> func, CancellationToken? cancellationToken = null, string taskName = null)
        {
            if (DebugAllTasks && taskName == null)
            {
                taskName = new StackTrace().GetFrame(1).GetMethod().Name;
            }

            // Need to box our result, no matter what it is.
            var pendingAction = new DeferredTask<object>(
                async (innerCancellationToken) =>
                {
                    var result = await func(innerCancellationToken);
                    return result;
                }, cancellationToken ?? CancellationToken.None);
            pendingAction.Name = taskName;

            PendingActions.Enqueue(pendingAction);
            NoonUtility.Log($"Queued coordinated task {taskName}");

            if (!isDrainingQueue)
            {
                DrainActions();
            }

            // Now we need to unbox it.  Sigh...
            return pendingAction.Task.ContinueWith<T>(
                (task, state) =>
                {
                    // Throw this independently so we do not wrap it in another exception.
                    // FIXME: Not working, we are still getting wrapped in what appears to be an AggregateException.
                    if (task.Exception != null)
                    {
                        throw task.Exception;
                    }

                    return (T)task.Result;
                },
                null,
                TaskContinuationOptions.ExecuteSynchronously);
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
                    // If the heart is not running, wait for a beat.
                    // If the heart is running, this no-ops.
                    await MechanicalHeart.AwaitBeat(CancellationToken.None, TimeSpan.Zero);

                    currentAction = set;
                    try
                    {
                        NoonUtility.Log($"Executing coordinated task {set.Name}");
                        await set.Execute();
                    }
                    catch (Exception ex)
                    {
                        NoonUtility.LogException(new Exception("Failed running a cerebellum action: " + ex.Message, ex));
                    }
                    finally
                    {
                        NoonUtility.Log($"Done executing coordinated task {set.Name}");
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
