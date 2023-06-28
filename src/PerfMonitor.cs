namespace AutoccultistNS
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static class PerfMonitor
    {
        private const int Samples = 1000;
        private const int AllocatedMsPerTask = 10;
        private static readonly TimeSpan SamplesPerSecondPeriod = TimeSpan.FromSeconds(3.0);

        private static readonly Dictionary<string, PerfEntry> EntriesByKey = new();

        public static IReadOnlyDictionary<string, PerfEntry> Entries => EntriesByKey;

        public static void ClearStatistics()
        {
            EntriesByKey.Clear();
        }

        public static T Monitor<T>(string key, Func<T> func)
        {
            var sw = Stopwatch.StartNew();
            var result = func();
            sw.Stop();
            AddSample(key, sw.Elapsed);
            return result;
        }

        public static void Monitor(string key, Action action)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            AddSample(key, sw.Elapsed);
        }

        public static void AddCount(string key)
        {
            if (!Entries.TryGetValue(key, out var entry))
            {
                entry = new PerfEntry();
                EntriesByKey.Add(key, entry);
            }

            entry.AddSample(TimeSpan.Zero);
        }

        public static void AddSample(string key, TimeSpan timeSpan)
        {
            if (!Entries.TryGetValue(key, out var entry))
            {
                entry = new PerfEntry();
                EntriesByKey.Add(key, entry);
            }

            entry.AddSample(timeSpan);
        }

        public class PerfEntry
        {
            private Queue<Sample> samples = new(PerfMonitor.Samples);

            public double SecondsSinceFirstSample
            {
                get
                {
                    if (this.samples.Count == 0)
                    {
                        return 0;
                    }

                    return (DateTime.Now - this.samples.Peek().Timestamp).TotalSeconds;
                }
            }

            public double SamplesPerSecond
            {
                get
                {
                    // Nonfrequent monitors can cause this to mantain lots of outliers, so restrict it to recent samples.
                    var recentSamples = this.samples.Where(x => (DateTime.Now - x.Timestamp) <= SamplesPerSecondPeriod).ToArray();
                    if (recentSamples.Length == 0)
                    {
                        return 0;
                    }

                    return recentSamples.Length / SamplesPerSecondPeriod.TotalSeconds;
                }
            }

            public double Average
            {
                get
                {
                    if (this.samples.Count == 0)
                    {
                        return 0;
                    }

                    return this.samples.Select(x => x.Timespan.TotalMilliseconds).Sum() / this.samples.Count;
                }
            }

            public double Max => this.samples.Select(x => x.Timespan.TotalMilliseconds).Max();

            // Total outliers divided by the total seconds since the first sample.
            public double GreaterThanAllocatedRate
            {
                get
                {
                    if (this.samples.Count == 0)
                    {
                        return 0;
                    }

                    return this.samples.Select(x => x.Timespan.TotalMilliseconds).Count(x => x > AllocatedMsPerTask) / this.SecondsSinceFirstSample;
                }
            }

            public void AddSample(TimeSpan timeSpan)
            {
                if (this.samples.Count >= PerfMonitor.Samples)
                {
                    this.samples.Dequeue();
                }

                this.samples.Enqueue(new Sample
                {
                    Timespan = timeSpan,
                    Timestamp = DateTime.Now,
                });
            }
        }

        private class Sample
        {
            public TimeSpan Timespan { get; set; }

            public DateTime Timestamp { get; set; }
        }
    }
}
