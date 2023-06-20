namespace AutoccultistNS.Brain
{
    /// <summary>
    /// Event sink for various events in the autoccultist brain.
    /// </summary>
    public static class BrainEventSink
    {
        /// <summary>
        /// Called when an operation is started.
        /// </summary>
        /// <param name="operation">The started operation.</param>
        public static void OnOperationStarted(IOperation operation)
        {
            Autoccultist.LogTrace("Starting operation " + operation.Name);
        }

        /// <summary>
        /// Called when an operation is completed.
        /// </summary>
        /// <param name="operation">The completed operation.</param>
        public static void OnOperationCompleted(IOperation operation)
        {
            Autoccultist.LogTrace($"Completing operation {operation.Name}");
        }

        /// <summary>
        /// Called when an operation is aborted.
        /// </summary>
        /// <param name="operation">The aborted operation.</param>
        public static void OnOperationAborted(IOperation operation)
        {
            Autoccultist.LogTrace($"Aborting operation {operation.Name}");
        }
    }
}
