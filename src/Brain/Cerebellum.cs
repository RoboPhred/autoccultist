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
        private const bool DebugTaskExecutions = false;

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

            if (DebugTaskExecutions && !string.IsNullOrEmpty(taskName))
            {
                NoonUtility.Log($"Queued coordinated task {taskName}");
            }

            if (!isDrainingQueue)
            {
                DrainActions();
            }

            return pendingAction.Task;
        }

        public static async Task<T> Coordinate<T>(Func<CancellationToken, Task<T>> func, CancellationToken? cancellationToken = null, string taskName = null)
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

            if (DebugTaskExecutions && !string.IsNullOrEmpty(taskName))
            {
                NoonUtility.Log($"Queued coordinated task {taskName}");
            }

            if (!isDrainingQueue)
            {
                DrainActions();
            }

            var result = (T)await pendingAction.Task;
            return result;
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
                        if (DebugTaskExecutions && !string.IsNullOrEmpty(set.Name))
                        {
                            NoonUtility.Log($"Executing coordinated task {set.Name}");
                        }

                        await set.Execute();
                    }
                    catch (Exception ex)
                    {
                        NoonUtility.LogException(new Exception("Failed running a cerebellum action: " + ex.Message, ex));
                    }
                    finally
                    {
                        if (DebugTaskExecutions && !string.IsNullOrEmpty(set.Name))
                        {
                            NoonUtility.Log($"Done executing coordinated task {set.Name}");
                        }

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
