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

        public static int HashAll<T>(IEnumerable<T> objects)
        {
            unchecked
            {
                // This is called a Berstein hash.  I don't understand it.  Nobody understands it.  Just live with it.
                int hash = 17;
                foreach (var obj in objects)
                {
                    hash = hash * 23 + obj?.GetHashCode() ?? 0;
                }
                return hash;
            }
        }

        public static int HashAllUnordered<T>(IEnumerable<T> objects)
        {
            unchecked
            {
                var codes = objects.Select(o => o?.GetHashCode() ?? 0).OrderBy(c => c);
                return HashAllUnordered(codes);
            }
        }

        public static int HashAllUnordered(IEnumerable<int> codes)
        {
            unchecked
            {
                int hash = 17;
                foreach (var code in codes)
                {
                    hash = hash * 23 + code;
                }
                return hash;
            }
        }
    }
}
