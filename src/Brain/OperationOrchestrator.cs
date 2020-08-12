using System;
using System.Collections.Generic;
using System.Linq;
using Autoccultist.Brain.Config;

namespace Autoccultist.Brain
{
    public static class OperationOrchestrator
    {
        private static IDictionary<string, OperationExecutor> executingOperationsBySituation = new Dictionary<string, OperationExecutor>();

        public static void Update()
        {
            foreach (var executor in executingOperationsBySituation.Values.ToArray())
            {
                executor.Update();
            }

            // After we update the existing situation handlers, dump any completed ones if any remain.
            //  This is because we need to let the solution executor dump to know its done, but
            //  other pop-up situations also need dumping
            // TODO: We should animate this through AutoccultistActor
            foreach (var situation in GameAPI.GetAllSituations())
            {
                if (situation.SituationClock.State == SituationState.Complete)
                {
                    situation.situationWindow.DumpAllResultingCardsToDesktop();
                }
            }
        }

        public static bool SituationIsAvailable(string situationId)
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
            return !executingOperationsBySituation.ContainsKey(situationId);
        }

        // This doesnt really belong here.
        //  On one hand, we can have multiple imperatives running at once, but on the other
        //  this just managed situations, and shouldn't care about what is running on them.
        public static void ExecuteOperation(Operation operation)
        {
            if (!SituationIsAvailable(operation.Situation))
            {
                throw new OperationFailedException($"Cannot execute operation for situation {operation.Situation} because the situation is not available.");
            }

            if (executingOperationsBySituation.ContainsKey(operation.Situation))
            {
                return;
            }

            var executor = new OperationExecutor(operation);
            executingOperationsBySituation[operation.Situation] = executor;
            executor.Completed += OnExecutorCompleted;
            executor.Start();
        }

        private static void OnExecutorCompleted(object sender, EventArgs e)
        {
            var executor = sender as OperationExecutor;
            executor.Completed -= OnExecutorCompleted;
            executingOperationsBySituation.Remove(executor.Operation.Situation);
        }
    }
}