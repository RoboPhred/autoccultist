namespace AutoccultistNS.Actor
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Tasks;

    /// <summary>
    /// The actor, that performs game actions on our behalf.
    /// <para>
    /// Game actions are delayed slightly, to make the gameplay easier to follow.
    /// </summary>
    public static class AutoccultistActor
    {
        private static readonly Queue<DeferredTask<bool>> PendingActions = new();
        private static DeferredTask<bool> currentAction;

        private static GameAPI.PauseToken pauseToken;

        private static bool isDrainingQueue = false;

        /// <summary>
        /// Queue up an asynchronous action for fifo execution.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">An optional CancellationToken used to cancel the action execution.</param>
        /// <returns>A task that will complete when all actions complete, or fail with an exception describing the action failure.</returns>
        public static Task Perform(Func<CancellationToken, Task> action, CancellationToken? cancellationToken = null)
        {
            var pendingAction = new DeferredTask<bool>(
                (innerCancellationToken) => action(innerCancellationToken).ContinueWith(_ => true),
                cancellationToken ?? CancellationToken.None);

            PendingActions.Enqueue(pendingAction);

            if (!isDrainingQueue)
            {
                DrainActions();
            }

            return pendingAction.Task;
        }

        /// <summary>
        /// Abort all ongoing actions.
        /// </summary>
        public static void AbortAllActions()
        {
            currentAction?.Cancel();
            currentAction = null;

            DeferredTask<bool> queuedTask;
            while ((queuedTask = PendingActions.DequeueOrDefault()) != null)
            {
                queuedTask.Cancel();
            }

            PendingActions.Clear();
        }

        public static async void DrainActions()
        {
            if (isDrainingQueue || PendingActions.Count == 0)
            {
                return;
            }

            OnActive();

            isDrainingQueue = true;
            try
            {
                DeferredTask<bool> set;
                while ((set = PendingActions.DequeueOrDefault()) != null)
                {
                    currentAction = set;
                    try
                    {
                        await set.Execute();
                    }
                    catch (Exception ex)
                    {
                        Autoccultist.Instance.LogWarn("Failed to perform action: " + ex.ToString());
                    }
                    finally
                    {
                        currentAction = null;
                    }

                    await MechanicalHeart.AwaitBeat(CancellationToken.None, AutoccultistSettings.ActionDelay);
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
            if (pauseToken == null)
            {
                pauseToken = GameAPI.Pause();
            }
        }

        private static void OnIdle()
        {
            pauseToken?.Dispose();
            pauseToken = null;

            if (AutoccultistSettings.SortTableOnIdle)
            {
                GameAPI.SortTable();
            }
        }
    }
}
