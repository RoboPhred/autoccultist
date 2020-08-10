using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Autoccultist.Hand
{
    static class AutoccultistHand
    {
        public static TimeSpan ActionDelay { get; set; } = TimeSpan.FromSeconds(0.4);
        private static DateTime lastUpdate = DateTime.Now;

        private static Queue<PendingActionSet> pendingActionSets = new Queue<PendingActionSet>();
        private static PendingActionSet currentActionSet;

        public static Task<bool> PerformActions(IEnumerable<IAutoccultistAction> actions, CancellationToken? cancellationToken = null)
        {
            var pendingAction = new PendingActionSet
            {
                PendingActions = actions.GetEnumerator(),
                TaskCompletion = new TaskCompletionSource<bool>(),
                CancellationToken = cancellationToken ?? CancellationToken.None
            };
            pendingActionSets.Enqueue(pendingAction);
            return pendingAction.TaskCompletion.Task;
        }

        public static void Update()
        {
            if (lastUpdate + ActionDelay > DateTime.Now)
            {
                // Not time to act yet.
                return;
            }

            // See if we need to get the next action set.
            if (currentActionSet == null)
            {
                currentActionSet = pendingActionSets.TryDequeue();
                if (currentActionSet == null)
                {
                    return;
                }

                // This is a new action set, start execution
                if (!currentActionSet.PendingActions.MoveNext())
                {
                    // Didnt have any actions, try again next time.
                    currentActionSet = null;
                    return;
                }

            }

            if (currentActionSet.CancellationToken.IsCancellationRequested)
            {
                // Task got cancelled.
                currentActionSet.TaskCompletion.TrySetCanceled();
                currentActionSet = null;
                return;
            }

            var nextAction = currentActionSet.PendingActions.Current;

            try
            {
                if (!nextAction.CanExecute())
                {
                    // Cant execute yet, dont add a delay but keep trying.
                    return;
                }

                // Execute the action, clear it out, and update the last update time
                //  to delay for the next action.
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

            // Immediately try to advance to the next pending action.
            // If there are no more pending actions, we need to know to set the completion result.
            if (!currentActionSet.PendingActions.MoveNext())
            {
                // No more actions, we are complete.
                currentActionSet.TaskCompletion.TrySetResult(true);
                currentActionSet = null;
            }
        }

        class PendingActionSet
        {
            public IEnumerator<IAutoccultistAction> PendingActions;
            public TaskCompletionSource<bool> TaskCompletion;
            public CancellationToken CancellationToken;
        }
    }
}