namespace AutoccultistNS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameState;

    /// <summary>
    /// Extensions to aspect dictionaries.
    /// </summary>
    public static class AspectExtensions
    {
        /// <summary>
        /// Gets the total value of all aspect degrees.
        /// </summary>
        /// <param name="aspects">The dictionary of aspects.</param>
        /// <param name="restrictTo">The list of aspects to restrict to.  If null, all aspects are included.</param>
        /// <returns>The total weight of all aspect degrees.</returns>
        public static double GetWeight(this IReadOnlyDictionary<string, int> aspects, ICollection<string> restrictTo = null)
        {
            return Math.Sqrt(aspects.Where(x => restrictTo == null || restrictTo.Contains(x.Key)).Select(x => x.Value * x.Value).Sum());
        }

        /// <summary>
        /// Determine if all matching aspects are satisfied.
        /// </summary>
        /// <param name="aspects">The aspects to check.</param>
        /// <param name="matchingAspects">The aspects to match against.</param>
        /// <param name="state">The current game state.</param>
        /// <returns>True if the aspects has all of the matching aspects with a minimum of their specified degrees.</returns>
        public static bool HasAspects(this IReadOnlyDictionary<string, int> aspects, IReadOnlyDictionary<string, IValueCondition> matchingAspects, IGameState state)
        {
            foreach (var entry in matchingAspects)
            {
                if (!aspects.TryGetValue(entry.Key, out int degree))
                {
                    degree = 0;
                }

                if (!entry.Value.IsConditionMet(degree, state))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
