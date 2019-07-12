using System.Collections.Generic;
using Autoccultist.Brain.Config;

namespace Autoccultist.Brain.Preconfigured
{
    public static class TestSolutions
    {
        public readonly static Imperative HardLaborSolution = new Imperative
        {
            SituationID = "work",
            StartingRecipeSolution = new RecipeSolution
            {
                SlotSolutions = new Dictionary<string, SlotSolution> {
                    {
                        "work",
                        new SlotSolution {
                            ElementID = "skillhealthc"
                        }
                    },
                    {
                        "Health",
                        new SlotSolution {
                            ElementID = "health"
                        }
                    }
                }
            }
        };
    }
}