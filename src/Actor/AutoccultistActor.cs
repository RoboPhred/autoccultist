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
        private static PendingActionSet currentActionSet;

        private static GameAPI.PauseToken pauseToken;

        private static bool isDrainingQueue = false;

        /// <summary>
        /// Gets or sets the delay between each action.
        /// </summary>
        public static TimeSpan ActionDelay { get; set; } = TimeSpan.FromSeconds(0.25);

        public static bool SortTableOnIdle { get; set; } = true;

        /// <summary>
        /// Perform the actions from the enumerable.
        /// </summary>
        /// <param name="actions">An enumerable of actions to execute.</param>
        /// <param name="cancellationToken">An optional CancellationToken used to cancel the action execution.</param>
        /// <returns>A task that will complete when all actions complete, or fail with an exception describing the action failure.</returns>
        public static Task PerformActions(IEnumerable<IAutoccultistAction> actions, CancellationToken? cancellationToken = null)
        {
            var pendingAction = new PendingActionSet(actions.GetEnumerator(), cancellationToken ?? CancellationToken.None);
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

            PendingActionSet pendingActionSet;
            while ((pendingActionSet = PendingActionSets.DequeueOrDefault()) != null)
            {
                pendingActionSet.Cancel();
            }

            PendingActionSets.Clear();
        }

        public static async void DrainActionSetQueue()
        {
            if (PendingActionSets.Count == 0)
            {
                return;
            }

            OnActive();

            isDrainingQueue = true;
            try
            {
                PendingActionSet set;
                while ((set = PendingActionSets.DequeueOrDefault()) != null)
                {
                    currentActionSet = set;
                    await set.Execute();
                    currentActionSet = null;
                }
            }
            finally
            {
                OnIdle();
                currentActionSet = null;
                isDrainingQueue = false;
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

            if (SortTableOnIdle)
            {
                GameAPI.SortTable();
            }
        }

        /// <summary>
        /// A pending action set.
        /// A PendingActionSet wrap ActionSet and keeps it on hold until Execute is called.
        /// Cancellation of a pending action set is also possible.
        /// </summary>
        private class PendingActionSet
        {
            private readonly TaskCompletionSource<bool> taskCompletionSource = new();
            private readonly IEnumerator<IAutoccultistAction> pendingActions;
            private readonly CancellationToken externalCancellationToken;
            private readonly CancellationTokenSource internalCancellationTokenSource = new();

            private bool didExecute = false;

            public PendingActionSet(IEnumerator<IAutoccultistAction> pendingActions, CancellationToken cancellationToken)
            {
                this.pendingActions = pendingActions;
                this.externalCancellationToken = cancellationToken;
            }

            public Task Task => this.taskCompletionSource.Task;

            public async Task Execute()
            {
                if (this.didExecute)
                {
                    throw new InvalidOperationException("This action set has already been executed.");
                }

                this.didExecute = true;

                var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(this.externalCancellationToken, this.internalCancellationTokenSource.Token).Token;
                cancellationToken.ThrowIfCancellationRequested();

                var set = new ActionSet(this.pendingActions, cancellationToken);
                try
                {
                    await set.AwaitCompletion();
                    this.taskCompletionSource.TrySetResult(true);
                }
                catch (TaskCanceledException)
                {
                    this.taskCompletionSource.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    this.taskCompletionSource.TrySetException(ex);
                }
            }

            public void Cancel()
            {
                this.internalCancellationTokenSource.Cancel();
                if (!this.didExecute)
                {
                    this.taskCompletionSource.TrySetCanceled();
                }
            }
        }

        private class ActionSet : AsyncUpdateTask<bool>
        {
            private readonly IEnumerator<IAutoccultistAction> actions;

            private DateTime lastUpdate;

            public ActionSet(IEnumerator<IAutoccultistAction> actions, CancellationToken cancellationToken)
                : base(cancellationToken)
            {
                this.actions = actions;
            }

            protected override void Update()
            {
                if (this.lastUpdate + ActionDelay > DateTime.Now)
                {
                    // Not time to act yet.
                    return;
                }

                if (!GameAPI.IsInteractable)
                {
                    // Not interactable at the moment
                    return;
                }

                ActionResult actionResult;
                do
                {
                    if (!this.actions.MoveNext())
                    {
                        this.SetComplete(true);
                        return;
                    }

                    var action = this.actions.Current;

                    // Execute the action, clear it out, and update the last update time
                    //  to delay for the next action.
                    Autoccultist.Instance.LogTrace($"Executing action {action}");
                    actionResult = action.Execute();
                }
                while (actionResult == ActionResult.NoOp);

                // We did the thing.  Set the last updated time so we can delay for the next action.
                lastUpdate = DateTime.Now;
            }
        }
    }
}
