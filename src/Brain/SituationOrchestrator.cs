namespace Autoccultist.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A static class responsible for mangaing all situation orchestrations.
    /// </summary>
    public static class SituationOrchestrator
    {
        private static readonly IDictionary<string, ISituationOrchestration> ExecutingOperationsBySituation = new Dictionary<string, ISituationOrchestration>();

        /// <summary>
        /// Gets all current orchestrations.
        /// </summary>
        public static IReadOnlyDictionary<string, ISituationOrchestration> CurrentOrchestrations
        {
            get
            {
                // Make a copy to ensure it is not mutated.
                return ExecutingOperationsBySituation.ToDictionary(entry => entry.Key, entry => entry.Value);
            }
        }

        /// <summary>
        /// Run all updates for the situation orchestrator.
        /// </summary>
        public static void Update()
        {
            foreach (var operation in ExecutingOperationsBySituation.Values.ToArray())
            {
                operation.Update();
            }

            // After we update the existing situation handlers, dump any completed ones if any remain.
            //  This is because some situations operate on their own volition, without an executor associated with them.
            foreach (var situation in GameAPI.GetAllSituations())
            {
                var situationId = situation.GetTokenId();

                if (ExecutingOperationsBySituation.ContainsKey(situationId))
                {
                    // Already orchestrating this
                    continue;
                }

                if (situation.SituationClock.State != SituationState.Complete)
                {
                    // Situation is ongoing, no need to dump it.
                    continue;
                }

                /*
                We still want to dump even if the situation has nothing in it
                This is particulary the case for situations that end with no output,
                such as suspicion-free suspicion
                */

                DumpSituation(situationId);
            }
        }

        /// <summary>
        /// Aborts all ongoing operations.
        /// </summary>
        public static void Abort()
        {
            foreach (var operation in ExecutingOperationsBySituation.Values.ToArray())
            {
                operation.Abort();
            }

            ExecutingOperationsBySituation.Clear();
        }

        /// <summary>
        /// Dumps the status of the situation orchestrator to the console.
        /// </summary>
        public static void LogStatus()
        {
            AutoccultistPlugin.Instance.LogInfo("We are orchestrating:");
            foreach (var entry in ExecutingOperationsBySituation)
            {
                AutoccultistPlugin.Instance.LogInfo($"-- {entry.Key}: {entry.Value}");
            }
        }

        /// <summary>
        /// Determines if the situation is available for use.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to check.</param>
        /// <returns>True if the situation is idle and available, False otherwise.</returns>
        public static bool IsSituationAvailable(string situationId)
        {
            var controller = GameAPI.GetSituation(situationId);
            if (controller == null)
            {
                // Not present on the board.
                return false;
            }

            if (controller.SituationClock.State != SituationState.Unstarted && controller.SituationClock.State != SituationState.Complete)
            {
                // Busy doing something.
                return false;
            }

            // Not busy, but we are still orchestrating it.
            return !ExecutingOperationsBySituation.ContainsKey(situationId);
        }

        /// <summary>
        /// Executes the given operation.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        public static void ExecuteOperation(IOperation operation)
        {
            if (!IsSituationAvailable(operation.Situation))
            {
                throw new OperationFailedException($"Cannot execute operation for situation {operation.Situation} because the situation is not available.");
            }

            if (ExecutingOperationsBySituation.ContainsKey(operation.Situation))
            {
                return;
            }

            var orchestration = new OperationOrchestration(operation);
            ExecutingOperationsBySituation[operation.Situation] = orchestration;
            orchestration.Completed += OnOrchestrationCompleted;
            orchestration.Start();
        }

        private static void DumpSituation(string situationId)
        {
            if (ExecutingOperationsBySituation.ContainsKey(situationId))
            {
                throw new OperationFailedException($"Cannot dump situation {situationId} because the situation already has an orchestration running.");
            }

            var orchestration = new DumpSituationOrchestration(situationId);
            ExecutingOperationsBySituation[situationId] = orchestration;
            orchestration.Completed += OnOrchestrationCompleted;
            orchestration.Start();
        }

        private static void OnOrchestrationCompleted(object sender, EventArgs e)
        {
            var orchestration = sender as ISituationOrchestration;
            orchestration.Completed -= OnOrchestrationCompleted;
            ExecutingOperationsBySituation.Remove(orchestration.SituationId);
        }
    }
}
