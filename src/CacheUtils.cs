namespace AutoccultistNS
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public static class CacheUtils
    {
        private const int MaxQueueLength = 1000;

        private static readonly ConditionalWeakTable<object, Dictionary<string, CacheData>> Values = new();

        private static readonly Queue<(int, DateTime)> CacheMisses = new();

        private static readonly Queue<(int, DateTime)> CacheHits = new();

        public static double MissesPerSecond => CacheMisses.Count / (DateTime.UtcNow - CacheMisses.PeekOrDefault().Item2).TotalSeconds;

        public static double HitsPerSecond => CacheHits.Count / (DateTime.UtcNow - CacheHits.PeekOrDefault().Item2).TotalSeconds;

        public static bool Enabled { get; set; } = true;

        public static void ClearStatistics()
        {
            CacheMisses.Clear();
            CacheHits.Clear();
        }

        public static TOutput Compute<TOutput>(object reference, string key, object input, Func<TOutput> func)
        {
            var cache = Values.GetOrCreateValue(reference);
            if (!cache.TryGetValue(key, out var data))
            {
                data = new CacheData();
                cache.Add(key, data);
            }

            var hashCode = input.GetHashCode();

            if (data.InputHashCode != hashCode)
            {
                if (CacheMisses.Count >= MaxQueueLength)
                {
                    CacheMisses.Dequeue();
                }

                CacheMisses.Enqueue((hashCode, DateTime.UtcNow));

                data.InputHashCode = hashCode;
                data.Value = func();

                // Sanity check
                if (data.Value is IEnumerable && !(data.Value is ICollection))
                {
                    throw new InvalidOperationException("CacheUtils.Compute returned an IEnumerable that is not an ICollection. This will likely cause crashes for re-enumeration.");
                }
            }
            else
            {
                if (CacheHits.Count >= MaxQueueLength)
                {
                    CacheHits.Dequeue();
                }

                CacheHits.Enqueue((hashCode, DateTime.UtcNow));
            }

            return (TOutput)data.Value;
        }

        private class CacheData
        {
            public int InputHashCode { get; set; }

            public object Value { get; set; }
        }
    }
}
