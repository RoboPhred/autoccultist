using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    public class SituationSolution
    {
        public string SituationID;

        public RecipeSolution StartingRecipeSolution;
        public IDictionary<string, RecipeSolution> OngoingRecipeSolutions;
    }
}