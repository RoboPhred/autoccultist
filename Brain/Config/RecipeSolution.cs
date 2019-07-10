using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    public class RecipeSolution
    {
        /// <summary>
        /// Delay from the start of the SituationSolution to the usage of this recipe.
        /// Used to calculate if expiring cards will be available.
        /// </summary>
        public int OngoingDelay;
        public IDictionary<string, SlotSolution> SlotSolutions;
    }
}