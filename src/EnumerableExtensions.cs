using System.Collections.Generic;
using System.Linq;

namespace AutoccultistNS
{
    public static class EnumerableExtensions
    {
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> target)
        {
            return !target.Except(source).Any();
        }
    }
}