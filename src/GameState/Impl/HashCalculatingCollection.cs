namespace AutoccultistNS.GameState.Impl
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class HashCalculatingCollection<T> : IReadOnlyCollection<T>, IContentHashable
    {
        private readonly IReadOnlyCollection<T> innerCollection;
        private readonly Lazy<int> hashCode;

        public HashCalculatingCollection(IEnumerable<T> items)
        {
            this.innerCollection = items.ToList();
            this.hashCode = new Lazy<int>(() => HashUtils.HashAllUnordered(items));
        }

        public int Count => this.innerCollection.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return this.innerCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int GetContentHash()
        {
            return this.hashCode.Value;
        }
    }
}
