namespace AutoccultistNS
{
    using System.Collections.Generic;

    public static class DictionaryExtensions
    {
        public static TValue ValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
    }
}
