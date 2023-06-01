namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameState;
    using SecretHistories.Enums;

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
            var state = GameStateProvider.Current;

            foreach (var operation in ExecutingOperationsBySituation.Values.ToArray())
            {
                operation.Update();
            }

            // After we update the existing situation handlers, dump any completed ones if any remain.
            //  This is because some situations operate on their own volition, without an executor associated with them.
            foreach (var situation in state.Situations)
            {
                if (ExecutingOperationsBySituation.ContainsKey(situation.SituationId))
                {
                    // Already orchestrating this
                    continue;
                }

                if (situation.State != StateEnum.Complete)
                {
                    // Situation is ongoing, no need to dump it.
                    continue;
                }

                /*
                We still want to dump even if the situation has nothing in it
                This is particulary the case for situations that end with no output,
                such as suspicion-free suspicion
                */

                DumpSituation(situation.SituationId);
            }
        }

        /// <summary>
        /// Aborts all ongoing operations.
        /// </summary>
        public static void AbortAll()
        {
            foreach (var operation in ExecutingOperationsBySituation.Values.ToArray())
            {
                operation.Abort();
            }

            ExecutingOperationsBySituation.Clear();
        }

        /// <summary>
        /// Determines if the situation is available for use.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to check.</param>
        /// <returns>True if the situation is idle and available, False otherwise.</returns>
        public static bool IsSituationAvailable(string situationId)
        {
            var situation = GameStateProvider.Current.Situations.FirstOrDefault(x => x.SituationId == situationId);
            if (situation == null)
            {
                return false;
            }

            if (situation.State != StateEnum.Unstarted && situation.State != StateEnum.Complete)
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

            if (GameStateProvider.Current.Mansus.State != PortalActiveState.Closed)
            {
                return;
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
