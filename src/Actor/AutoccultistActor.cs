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
        private static readonly Queue<DeferredTask<bool>> PendingActionSets = new();
        private static DeferredTask<bool> currentActionSet;

        private static GameAPI.PauseToken pauseToken;

        private static bool isDrainingQueue = false;

        public static bool SortTableOnIdle { get; set; } = true;

        /// <summary>
        /// Perform the actions from the enumerable.
        /// </summary>
        /// <param name="actions">An enumerable of actions to execute.</param>
        /// <param name="cancellationToken">An optional CancellationToken used to cancel the action execution.</param>
        /// <returns>A task that will complete when all actions complete, or fail with an exception describing the action failure.</returns>
        public static Task PerformActions(IEnumerable<IAutoccultistAction> actions, CancellationToken? cancellationToken = null)
        {
            var pendingAction = new DeferredTask<bool>(
                (innerCancellationToken) => ExecuteActionSet(actions.GetEnumerator(), innerCancellationToken),
                cancellationToken ?? CancellationToken.None);

            PendingActionSets.Enqueue(pendingAction);

            if (!isDrainingQueue)
            {
                DrainActionSetQueue();
            }

            return pendingAction.Task;
        }

        /// <summary>
        /// Abort all ongoing actions.
        /// </summary>
        public static void AbortAllActions()
        {
            currentActionSet?.Cancel();
            currentActionSet = null;

            DeferredTask<bool> queuedTask;
            while ((queuedTask = PendingActionSets.DequeueOrDefault()) != null)
            {
                queuedTask.Cancel();
            }

            PendingActionSets.Clear();
        }

        public static async void DrainActionSetQueue()
        {
            if (isDrainingQueue || PendingActionSets.Count == 0)
            {
                return;
            }

            OnActive();

            isDrainingQueue = true;
            try
            {
                DeferredTask<bool> set;
                while ((set = PendingActionSets.DequeueOrDefault()) != null)
                {
                    currentActionSet = set;
                    await set.Execute();
                    currentActionSet = null;
                }
            }
            finally
            {
                currentActionSet = null;
                isDrainingQueue = false;
                OnIdle();
            }
        }

        private static async Task<bool> ExecuteActionSet(IEnumerator<IAutoccultistAction> actions, CancellationToken cancellationToken)
        {
            while (actions.MoveNext())
            {
                var action = actions.Current;
                Autoccultist.Instance.LogTrace($"Executing action {action}");
                var result = await action.Execute(cancellationToken);
                if (result != ActionResult.NoOp)
                {
                    await new EngineDelayTask(AutoccultistSettings.ActionDelay, cancellationToken).Task;
                }
            }

            return true;
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

            if (SortTableOnIdle)
            {
                GameAPI.SortTable();
            }
        }
    }
}
