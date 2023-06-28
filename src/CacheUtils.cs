using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AutoccultistNS
{
    public static class CacheUtils
    {
        private const int MaxQueueLength = 1000;

        private static Queue<(int, DateTime)> cacheMisses = new();

        private static Queue<(int, DateTime)> cacheHits = new();

        private static readonly ConditionalWeakTable<object, CacheData> values = new();

        public static double MissesPerSecond => cacheMisses.Count / (DateTime.UtcNow - cacheMisses.Peek().Item2).TotalSeconds;

        public static double HitsPerSecond => cacheHits.Count / (DateTime.UtcNow - cacheHits.Peek().Item2).TotalSeconds;

        public static void ClearStatistics()
        {
            cacheMisses.Clear();
            cacheHits.Clear();
        }

        public static TOutput Compute<TInput, TOutput>(object cacheKey, TInput input, Func<TInput, TOutput> func)
        {
            var hashCode = input.GetHashCode();
            var data = values.GetOrCreateValue(cacheKey);
            if (data.InputHashCode != hashCode)
            {
                if (cacheMisses.Count >= MaxQueueLength)
                {
                    cacheMisses.Dequeue();
                }
                cacheMisses.Enqueue((hashCode, DateTime.UtcNow));

                data.InputHashCode = hashCode;
                data.Value = func(input);
            }
            else
            {
                if (cacheHits.Count >= MaxQueueLength)
                {
                    cacheHits.Dequeue();
                }

                cacheHits.Enqueue((hashCode, DateTime.UtcNow));
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
