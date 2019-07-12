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

        public void ExecuteSituationSolution(Imperative situationSolution)
        {
            if (this.executingSituations.ContainsKey(situationSolution.SituationID))
            {
                return;
            }

            var executor = new SituationSolutionExecutor(situationSolution);
            this.executingSituations[situationSolution.SituationID] = executor;
            executor.Completed += this.OnExecutorCompleted;
            executor.Start();
        }

        private void OnExecutorCompleted(object sender, EventArgs e)
        {
            var executor = sender as SituationSolutionExecutor;
            executor.Completed -= this.OnExecutorCompleted;
            this.executingSituations.Remove(executor.Solution.SituationID);
        }
    }
}