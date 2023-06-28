namespace AutoccultistNS
{
    using System.Collections.Generic;
    using System.Linq;

    public static class HashUtils
    {
        /// <summary>
        /// Computes a hash code for the given objects.
        /// </summary>
        /// <param name="objects">The objects to hash.</param>
        public static int Hash(params object[] objects)
        {
            unchecked
            {
                return HashAll(objects);
            }
        }

        public static int HashAllUnordered<T>(IEnumerable<T> objects)
        {
            unchecked
            {
                return HashAllUnordered(objects.Select(HashOrDefault));
            }
        }

        public static int HashAllUnordered(IEnumerable<int> codes)
        {
            unchecked
            {
                return HashAll(codes.OrderBy(c => c));
            }
        }

        public static int HashAll<T>(IEnumerable<T> objects)
        {
            unchecked
            {
                return HashAll(objects.Select(HashOrDefault));
            }
        }

        public static int HashAll(IEnumerable<int> codes)
        {
            unchecked
            {
                // This is called a Berstein hash.  I don't understand it.  Nobody understands it.  Just live with it.
                int hash = 17;
                foreach (var code in codes)
                {
                    hash = (hash * 23) + code;
                }

                return hash;
            }
        }

        public static int HashOrDefault<T>(T obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}
