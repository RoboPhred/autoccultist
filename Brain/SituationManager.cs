using System;
using System.Collections.Generic;
using Autoccultist.Brain.Config;

namespace Autoccultist.Brain
{
    public class SituationManager
    {
        private IDictionary<string, SituationSolutionExecutor> executingSituations = new Dictionary<string, SituationSolutionExecutor>();

        public bool SituationIsAvailable(string id)
        {
            return !this.executingSituations.ContainsKey(id);
        }

        public void ClearCompletedSituations()
        {
            var situations = GameAPI.GetAllSituations();
            foreach (var situation in situations)
            {
                if (situation.SituationClock.State == SituationState.Complete)
                {
                    situation.situationWindow.DumpAllResultingCardsToDesktop();
                }
            }
        }

        public void ExecuteSituationSolution(Imperative situationSolution)
        {
            if (this.executingSituations.ContainsKey(situationSolution.Verb))
            {
                return;
            }

            var executor = new SituationSolutionExecutor(situationSolution);
            this.executingSituations[situationSolution.Verb] = executor;
            executor.Completed += this.OnExecutorCompleted;
            executor.Start();
        }

        private void OnExecutorCompleted(object sender, EventArgs e)
        {
            var executor = sender as SituationSolutionExecutor;
            executor.Completed -= this.OnExecutorCompleted;
            this.executingSituations.Remove(executor.Solution.Verb);
        }
    }
}