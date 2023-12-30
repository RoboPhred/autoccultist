namespace AutoccultistNS
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> target)
        {
            return !target.Except(source).Any();
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> enumerable, int count)
        {
            var window = new List<T>(count);
            foreach (var item in enumerable)
            {
                window.Add(item);
                if (window.Count == count)
                {
                    yield return window;
                    window.Clear();
                }
            }

            if (window.Count > 0)
            {
                yield return window;
            }
        }
    }
}
