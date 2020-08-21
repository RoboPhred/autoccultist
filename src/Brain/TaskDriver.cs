namespace Autoccultist.Brain
{
    using System.Collections.Generic;

    /// <summary>
    /// Executes goals in order.
    /// <para>
    /// Perhaps some day, we will have parallel and looping goal tasks.
    /// </summary>
    public static class TaskDriver
    {
        private static readonly Queue<IGoal> GoalQueue = new Queue<IGoal>();

        private static IGoal currentGoal;

        static TaskDriver()
        {
            GoalDriver.OnGoalCompleted += OnGoalCompleted;
        }

        /// <summary>
        /// Gets a value indicating whether the task driver is running.
        /// </summary>
        public static bool IsRunning { get; private set; }

        /// <summary>
        /// Sets the task list for the TaskDriver to execute.
        /// </summary>
        /// <param name="tasks">The tasks to execute.</param>
        public static void SetTasks(IReadOnlyList<IGoal> tasks)
        {
            TryStopGoal();

            GoalQueue.Clear();
            foreach (var task in tasks)
            {
                GoalQueue.Enqueue(task);
            }

            if (IsRunning)
            {
                TryStartGoal();
            }
        }

        /// <summary>
        /// Resets the TaskDriver and clears all tasks.
        /// </summary>
        public static void Reset()
        {
            GoalQueue.Clear();
        }

        /// <summary>
        /// Starts the TaskDriver executing its tasks.
        /// </summary>
        public static void Start()
        {
            IsRunning = true;
            TryStartGoal();
        }

        /// <summary>
        /// Stops the TaskDriver from executing its tasks.
        /// </summary>
        public static void Stop()
        {
            IsRunning = false;
            TryStopGoal();
        }

        private static void TryStartGoal()
        {
            if (!IsRunning)
            {
                return;
            }

            if (currentGoal != null)
            {
                return;
            }

            currentGoal = GoalQueue.DequeueOrDefault();
            if (currentGoal == null)
            {
                AutoccultistPlugin.Instance.LogTrace("TaskDriver: No goal to start.");
                return;
            }

            AutoccultistPlugin.Instance.LogTrace("TaskDriver: Adding goal " + currentGoal.Name);
            GoalDriver.AddGoal(currentGoal);
        }

        private static void TryStopGoal()
        {
            if (currentGoal != null)
            {
                GoalDriver.RemoveGoal(currentGoal);
            }

            currentGoal = null;
        }

        private static void OnGoalCompleted(object sender, GoalCompletedEventArgs e)
        {
            if (e.CompletedGoal != currentGoal)
            {
                return;
            }

            AutoccultistPlugin.Instance.LogTrace("TaskDriver: Current goal " + currentGoal.Name + " completed.");
            currentGoal = null;
            TryStartGoal();
        }
    }
}
