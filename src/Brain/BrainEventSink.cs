namespace AutoccultistNS.Brain
{
    /// <summary>
    /// Event sink for various events in the autoccultist brain.
    /// </summary>
    public static class BrainEventSink
    {
        /// <summary>
        /// Called when a goal is started.
        /// </summary>
        /// <param name="goal">The started goal.</param>
        public static void OnGoalStarted(IGoal goal)
        {
            Autoccultist.Instance.LogInfo($"Starting goal {goal.Name}.");
        }

        /// <summary>
        /// Called when a goal is completed.
        /// </summary>
        /// <param name="goal">The completed goal.</param>
        public static void OnGoalCompleted(IGoal goal)
        {
            Autoccultist.Instance.LogInfo($"Goal {goal.Name} is now complete.");
        }

        /// <summary>
        /// Called when an operation is started.
        /// </summary>
        /// <param name="operation">The started operation.</param>
        public static void OnOperationStarted(IOperation operation)
        {
            Autoccultist.Instance.LogTrace("Starting operation " + operation.Name);
        }

        /// <summary>
        /// Called when an operation is completed.
        /// </summary>
        /// <param name="operation">The completed operation.</param>
        public static void OnOperationCompleted(IOperation operation)
        {
            Autoccultist.Instance.LogTrace($"Completing operation {operation.Name}");
        }

        /// <summary>
        /// Called when an operation is aborted.
        /// </summary>
        /// <param name="operation">The aborted operation.</param>
        public static void OnOperationAborted(IOperation operation)
        {
            Autoccultist.Instance.LogTrace($"Aborting operation {operation.Name}");
        }
    }
}
