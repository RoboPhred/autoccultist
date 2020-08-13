using System.Collections.Generic;
using System.Linq;

namespace Autoccultist
{
    public static class AspectsContainerExtensions
    {
        public static int GetAspectWeight(this IReadOnlyDictionary<string, int> container)
        {
            return container.Values.Sum();
        }

        public static bool MatchesAspects(this IReadOnlyDictionary<string, int> container, IReadOnlyDictionary<string, int> aspects)
        {
            foreach (var entry in aspects)
            {
                if (!container.TryGetValue(entry.Key, out var degree))
                {
                    return false;
                }
                if (degree < entry.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}