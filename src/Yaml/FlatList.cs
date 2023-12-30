namespace AutoccultistNS.Yaml
{
    using System.Collections.Generic;

    /// <summary>
    /// A configuration class that can load and flatten a nested array of items
    /// </summary>
    /// <typeparam name="T">The type of item in the list.</typeparam>
    public class FlatList<T> : List<T>
    {
        public FlatList()
        {
        }

        public FlatList(IEnumerable<T> items)
            : base(items)
        {
        }
    }
}
