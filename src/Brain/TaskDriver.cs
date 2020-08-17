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

        public static bool IsRunning { get; private set; }

        private static IGoal currentGoal;

        static TaskDriver()
        {
            GoalDriver.OnGoalCompleted += OnGoalCompleted;
        }

        public static void SetTasks(IList<IGoal> tasks)
        {
            TryStopGoal();
            GoalQueue.Clear();
            foreach (var task in tasks)
            {
                GoalQueue.Enqueue(task);
            }
        }

        public static void Start()
        {
            IsRunning = true;
            TryStartGoal();
        }

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
                return;
            }

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

            currentGoal = null;
            TryStartGoal();
        }
    }
}
