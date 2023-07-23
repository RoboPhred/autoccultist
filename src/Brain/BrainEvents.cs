namespace AutoccultistNS.Brain
{
    using System;

    /// <summary>
    /// Event sink for various events in the autoccultist brain.
    /// </summary>
    public static class BrainEvents
    {
        public static event EventHandler<OperationEventArgs> OperationStarted;

        public static event EventHandler<OperationEventArgs> OperationCompleted;

        public static event EventHandler<OperationEventArgs> OperationAborted;

        /// <summary>
        /// Called when an operation is started.
        /// </summary>
        /// <param name="operation">The started operation.</param>
        public static void OnOperationStarted(IOperation operation)
        {
            Autoccultist.LogTrace($"Starting operation {operation}");
            OperationStarted?.Invoke(null, new OperationEventArgs(operation));
        }

        /// <summary>
        /// Called when an operation is completed.
        /// </summary>
        /// <param name="operation">The completed operation.</param>
        public static void OnOperationCompleted(IOperation operation)
        {
            Autoccultist.LogTrace($"Completing operation {operation}");
            OperationCompleted?.Invoke(null, new OperationEventArgs(operation));
        }

        /// <summary>
        /// Called when an operation is aborted.
        /// </summary>
        /// <param name="operation">The aborted operation.</param>
        public static void OnOperationAborted(IOperation operation)
        {
            Autoccultist.LogTrace($"Aborting operation {operation}");
            OperationAborted?.Invoke(null, new OperationEventArgs(operation));
        }
    }
}
