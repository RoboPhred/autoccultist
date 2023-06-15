using System.Collections.Generic;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;

namespace AutoccultistNS
{
    public static class SituationExtensions
    {
        public static IReadOnlyList<Sphere> GetCurrentThresholdSpheres(this Situation situation)
        {
            // Note: Used to be situation.GetSpheresByCategory(SphereCategory.Threshold), but that seemingly returns an unstable order.
            // Chel says it should be otherwise, but bot is slotting things randomly.
            // Trying this instead at Chel's suggestion.

            if (situation.State.Identifier == StateEnum.Ongoing)
            {
                return situation.GetDominion(SituationDominionEnum.RecipeThresholds).Spheres;
            }
            else if (situation.State.Identifier == StateEnum.Unstarted || situation.State.Identifier == StateEnum.RequiringExecution)
            {
                return situation.GetDominion(SituationDominionEnum.VerbThresholds).Spheres;
            }

            // Fall back to the original.
            return situation.GetSpheresByCategory(SphereCategory.Threshold);
        }
    }
}