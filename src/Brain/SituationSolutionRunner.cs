using System;
using System.Collections.Generic;
using System.Linq;
using Autoccultist.Brain.Config;

namespace Autoccultist.Brain
{
    public static class SituationSolutionRunner
    {
        private static IDictionary<string, SituationSolutionExecutor> executingSituations = new Dictionary<string, SituationSolutionExecutor>();

        public static void Update()
        {
            foreach (var situation in executingSituations.Values.ToArray())
            {
                situation.Update();
            }

            // After we update the existing situation handlers, dump any completed ones if any remain.
            //  This is because we need to let the solution executor dump to know its done, but
            //  other pop-up situations also need dumping
            foreach (var situation in GameAPI.GetAllSituations())
            {
                if (situation.SituationClock.State == SituationState.Complete)
                {
                    situation.situationWindow.DumpAllResultingCardsToDesktop();
                }
            }
        }

        public static bool SituationIsAvailable(string id)
        {
            var controller = GameAPI.GetSituation(id);
            if (controller == null)
            {
                return false;
            }
            if (controller.SituationClock.State != SituationState.Unstarted && controller.SituationClock.State != SituationState.Complete)
            {
                return false;
            }

            return !executingSituations.ContainsKey(id);
        }

        // This doesnt really belong here.
        //  On one hand, we can have multiple imperatives running at once, but on the other
        //  this just managed situations, and shouldn't care about what is running on them.
        public static void ExecuteSituationSolution(Imperative situationSolution)
        {
            if (executingSituations.ContainsKey(situationSolution.Situation))
            {
                return;
            }

            var executor = new SituationSolutionExecutor(situationSolution);
            executingSituations[situationSolution.Situation] = executor;
            executor.Completed += OnExecutorCompleted;
            executor.Start();
        }

        private static void OnExecutorCompleted(object sender, EventArgs e)
        {
            var executor = sender as SituationSolutionExecutor;
            executor.Completed -= OnExecutorCompleted;
            executingSituations.Remove(executor.Solution.Situation);
        }
    }
}