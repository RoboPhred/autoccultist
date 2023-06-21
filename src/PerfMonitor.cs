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

        private static readonly Dictionary<string, PerfEntry> _Entries = new();

        public static IReadOnlyDictionary<string, PerfEntry> Entries => _Entries;

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

        private static void AddSample(string key, TimeSpan timeSpan)
        {
            if (!Entries.TryGetValue(key, out var entry))
            {
                entry = new PerfEntry();
                _Entries.Add(key, entry);
            }

            entry.AddSample(timeSpan);
        }

        public class PerfEntry
        {
            private Queue<Sample> samples = new(PerfMonitor.Samples);

            public double SecondsSinceFirstSample => (DateTime.Now - this.samples.Peek()?.Timestamp)?.TotalSeconds ?? 1;

            public double SamplesPerSecond => this.samples.Count / this.SecondsSinceFirstSample;

            public double Average => this.samples.Select(x => x.Timespan.TotalMilliseconds).Sum() / this.samples.Count;

            public double Max => this.samples.Select(x => x.Timespan.TotalMilliseconds).Max();

            // Total outliers divided by the total seconds since the first sample.
            public double GreaterThanAllocatedRate => this.samples.Select(x => x.Timespan.TotalMilliseconds).Count(x => x > AllocatedMsPerTask) / this.SecondsSinceFirstSample;

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
