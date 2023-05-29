namespace AutoccultistNS.Actor
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The actor, that performs game actions on our behalf.
    /// <para>
    /// Game actions are delayed slightly, to make the gameplay easier to follow.
    /// </summary>
    public static class AutoccultistActor
    {
        private static readonly Queue<PendingActionSet> PendingActionSets = new();

        private static GameAPI.PauseToken pauseToken;

        private static DateTime lastUpdate = DateTime.Now;
        private static PendingActionSet currentActionSet;

        /// <summary>
        /// Gets or sets the delay between each action.
        /// </summary>
        public static TimeSpan ActionDelay { get; set; } = TimeSpan.FromSeconds(0.25);

        /// <summary>
        /// Perform the actions from the enumerable.
        /// </summary>
        /// <param name="actions">An enumerable of actions to execute.</param>
        /// <param name="cancellationToken">An optional CancellationToken used to cancel the action execution.</param>
        /// <returns>A task that will complete when all actions complete, or fail with an exception describing the action failure.</returns>
        public static Task<ActorResult> PerformActions(IEnumerable<IAutoccultistAction> actions, CancellationToken? cancellationToken = null)
        {
            var pendingAction = new PendingActionSet
            {
                PendingActions = actions.GetEnumerator(),
                TaskCompletion = new TaskCompletionSource<ActorResult>(),
                CancellationToken = cancellationToken ?? CancellationToken.None,
            };
            PendingActionSets.Enqueue(pendingAction);
            return pendingAction.TaskCompletion.Task;
        }

        /// <summary>
        /// Abort all ongoing actions.
        /// </summary>
        public static void AbortAllActions()
        {
            PendingActionSet pendingActionSet;
            while ((pendingActionSet = PendingActionSets.DequeueOrDefault()) != null)
            {
                pendingActionSet.TaskCompletion.TrySetCanceled();
            }

            PendingActionSets.Clear();
            currentActionSet = null;
        }

        /// <summary>
        /// Run frame updates for the actor.
        /// </summary>
        public static void Update()
        {
            if (!GameAPI.IsInteractable)
            {
                // Not interactable at the moment
                return;
            }

            if (lastUpdate + ActionDelay > DateTime.Now)
            {
                // Not time to act yet.
                return;
            }

            if (!EnsureActionSet())
            {
                return;
            }

            if (currentActionSet.CancellationToken.IsCancellationRequested)
            {
                // Task got cancelled.
                currentActionSet.TaskCompletion.TrySetCanceled();
                currentActionSet = null;
                return;
            }

            try
            {
                // Advance to the next pending action.
                if (!currentActionSet.PendingActions.MoveNext())
                {
                    // No more actions, we are complete.
                    currentActionSet.TaskCompletion.TrySetResult(ActorResult.Success);
                    currentActionSet = null;
                }
            }
            catch (Exception ex)
            {
                // Failed to do whatever it is it wants to do, the entire action set is now dead.
                currentActionSet.TaskCompletion.TrySetException(ex);
                currentActionSet = null;
            }

            var nextAction = currentActionSet.PendingActions.Current;

            // We now have something to do
            OnActive();

            try
            {
                // Execute the action, clear it out, and update the last update time
                //  to delay for the next action.
                Autoccultist.Instance.LogTrace($"Executing action {nextAction}");
                nextAction.Execute();
            }
            catch (Exception ex)
            {
                // Failed to do whatever it is it wants to do, the entire action set is now dead.
                currentActionSet.TaskCompletion.TrySetException(ex);
                currentActionSet = null;
                return;
            }

            // We did the thing.  Set the last updated time so we can delay for the next action.
            lastUpdate = DateTime.Now;

            // Note: This might have been the last action in the action set.
            // Originally, we advanced the iterator here, so we could detect that and immediately mark it as complete.
            // However, if we did have another action, that action would be formed and queued based on out of date state information.
            // Instead, we now call MoveNext when we want to execute something, which means we have an up to date action but also means
            // completion of action sets is delayed by one actor tick.
        }

        private static bool EnsureActionSet()
        {
            // See if we need to get the next action set.
            if (currentActionSet == null)
            {
                currentActionSet = PendingActionSets.DequeueOrDefault();
                if (currentActionSet == null)
                {
                    // No more action sets
                    OnIdle();
                    return false;
                }
            }

            return currentActionSet != null;
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
        }

        private class PendingActionSet
        {
            /// <summary>
            /// Gets or sets the enumerator of pending actions.
            /// </summary>
            public IEnumerator<IAutoccultistAction> PendingActions { get; set; }

            /// <summary>
            /// Gets or sets the task completion source to report back when the actions are completed.
            /// </summary>
            public TaskCompletionSource<ActorResult> TaskCompletion { get; set; }

            /// <summary>
            /// Gets or sets the cancellation token that can cancel this action set.
            /// </summary>
            public CancellationToken CancellationToken { get; set; }
        }
    }
}
