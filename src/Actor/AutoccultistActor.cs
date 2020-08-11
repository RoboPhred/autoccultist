using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Autoccultist.Actor
{
    static class AutoccultistActor
    {
        public static TimeSpan ActionDelay { get; set; } = TimeSpan.FromSeconds(0.2);
        private static DateTime lastUpdate = DateTime.Now;

        private static Queue<PendingActionSet> pendingActionSets = new Queue<PendingActionSet>();
        private static PendingActionSet currentActionSet;

        public static Task<ActorResult> PerformActions(IEnumerable<IAutoccultistAction> actions, CancellationToken? cancellationToken = null)
        {
            var pendingAction = new PendingActionSet
            {
                PendingActions = actions.GetEnumerator(),
                TaskCompletion = new TaskCompletionSource<ActorResult>(),
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
                // Initially, this waited until we could execute to execute,
                //  but because things can happen that are never recoverable,
                //  we should throw instead.
                // if (!nextAction.CanExecute())
                // {
                //     return;
                // }

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
                currentActionSet.TaskCompletion.TrySetResult(ActorResult.Success);
                currentActionSet = null;
            }
        }

        class PendingActionSet
        {
            public IEnumerator<IAutoccultistAction> PendingActions;
            public TaskCompletionSource<ActorResult> TaskCompletion;
            public CancellationToken CancellationToken;
        }
    }
}