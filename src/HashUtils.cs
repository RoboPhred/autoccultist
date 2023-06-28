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
                    hash = hash * 23 + HashOrDefault(obj);
                }
                return hash;
            }
        }

        public static int HashAllUnordered<T>(IEnumerable<T> objects)
        {
            unchecked
            {
                var codes = objects.Select(o => HashOrDefault(o)).OrderBy(c => c);
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

        public static int HashOrDefault(object obj)
        {
            // WARN: The unity engine has something against `obj?.GetHashCode() ?? 0`.
            // It crashes with null refs in Roost, and it crashes with null refs here.

            if (obj == null)
            {
                return 0;
            }

            return obj.GetHashCode();
        }
    }
}
