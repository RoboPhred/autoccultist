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

        public static IReadOnlyDictionary<TKey, TValue> Merge<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, IReadOnlyDictionary<TKey, TValue> other)
        {
            var result = new Dictionary<TKey, TValue>();

            foreach (var kvp in dict)
            {
                result[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in other)
            {
                result[kvp.Key] = kvp.Value;
            }

            return result;
        }
    }
}
