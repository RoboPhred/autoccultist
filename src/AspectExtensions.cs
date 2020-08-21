namespace Autoccultist
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extensions to aspect dictionaries.
    /// </summary>
    public static class AspectExtensions
    {
        /// <summary>
        /// Gets the total value of all aspect degrees.
        /// </summary>
        /// <param name="aspects">The dictionary of aspects.</param>
        /// <returns>The total weight of all aspect degrees.</returns>
        public static int GetWeight(this IReadOnlyDictionary<string, int> aspects)
        {
            return aspects.Values.Sum();
        }

        /// <summary>
        /// Determine if all matching aspects are satisfied.
        /// </summary>
        /// <param name="aspects">The aspects to check.</param>
        /// <param name="matchingAspects">The aspects to match against.</param>
        /// <returns>True if the aspects has all of the matching aspects with a minimum of their specified degrees.</returns>
        public static bool HasAspects(this IReadOnlyDictionary<string, int> aspects, IReadOnlyDictionary<string, IValueCondition> matchingAspects)
        {
            foreach (var entry in matchingAspects)
            {
                if (!aspects.TryGetValue(entry.Key, out int degree))
                {
                    degree = 0;
                }

                if (!entry.Value.IsConditionMet(degree))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
