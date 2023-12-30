namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using AutoccultistNS.GameState;

    /// <summary>
    /// Event sink for various events in the autoccultist brain.
    /// </summary>
    public static class BrainEvents
    {
        public static event EventHandler<OperationEventArgs> OperationStarted;

        public static event EventHandler<OperationRecipeEventArgs> OperationRecipeExecuted;

        public static event EventHandler<OperationCompletedEventArgs> OperationCompleted;

        public static event EventHandler<OperationEventArgs> OperationAborted;

        public static event EventHandler<OperationEventArgs> OperationEnded;

        /// <summary>
        /// Called when an operation is started.
        /// </summary>
        /// <param name="operation">The started operation.</param>
        public static void OnOperationStarted(IOperation operation)
        {
            Autoccultist.LogTrace($"Starting operation {operation}");
            OperationStarted?.Invoke(null, new OperationEventArgs(operation));
        }

        public static void OnOperationRecipeExecuted(IOperation operation, string recipeName, IReadOnlyDictionary<string, ICardState> slottedCards)
        {
            Autoccultist.LogTrace($"Executing recipe {recipeName} for operation {operation}");
            OperationRecipeExecuted?.Invoke(null, new OperationRecipeEventArgs(operation, recipeName, slottedCards));
        }

        /// <summary>
        /// Called when an operation is completed.
        /// </summary>
        /// <param name="operation">The completed operation.</param>
        public static void OnOperationCompleted(IOperation operation, IReadOnlyCollection<ICardState> outputCards)
        {
            Autoccultist.LogTrace($"Completing operation {operation}");
            OperationCompleted?.Invoke(null, new OperationCompletedEventArgs(operation, outputCards));
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

        public static void OnOperationEnded(IOperation operation)
        {
            OperationEnded?.Invoke(null, new OperationEventArgs(operation));
        }
    }
}
