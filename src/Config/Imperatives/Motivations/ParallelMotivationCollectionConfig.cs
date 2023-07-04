namespace AutoccultistNS.Config
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using YamlDotNet.Core;

    public class ParallelMotivationCollectionConfig : MotivationCollectionConfig, IDictionary<string, ParallelMotivationCollectionConfig.ParallelMotivationConfig>
    {
        private readonly Dictionary<string, ParallelMotivationConfig> motivations = new();

        public ParallelMotivationCollectionConfig()
            : base()
        {
        }

        private enum MotivationStatus
        {
            Processing,
            MissingRequirements,
            CanRun,
            Satisfied,
        }

        public ICollection<string> Keys => this.motivations.Keys;

        public ICollection<ParallelMotivationConfig> Values => this.motivations.Values;

        public override int Count => this.motivations.Count;

        public bool IsReadOnly => false;

        public override IReadOnlyCollection<IImperative> Children => this.motivations.Values;

        public ParallelMotivationConfig this[string key] { get => this.motivations[key]; set => this.motivations[key] = value; }

        public override ConditionResult IsConditionMet(IGameState state)
        {
            return ConditionResult.Success;
        }

        public IEnumerator<KeyValuePair<string, ParallelMotivationConfig>> GetEnumerator()
        {
            return this.motivations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.motivations.GetEnumerator();
        }

        public void Add(string key, ParallelMotivationConfig value)
        {
            this.motivations.Add(key, value);
        }

        public void Add(KeyValuePair<string, ParallelMotivationConfig> item)
        {
            this.motivations.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.motivations.Clear();
        }

        public bool Contains(KeyValuePair<string, ParallelMotivationConfig> item)
        {
            return this.motivations.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return this.motivations.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, ParallelMotivationConfig>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(string key)
        {
            return this.motivations.Remove(key);
        }

        public bool Remove(KeyValuePair<string, ParallelMotivationConfig> item)
        {
            return this.motivations.Remove(item.Key);
        }

        public bool TryGetValue(string key, out ParallelMotivationConfig value)
        {
            return this.motivations.TryGetValue(key, out value);
        }

        IEnumerator<KeyValuePair<string, ParallelMotivationConfig>> IEnumerable<KeyValuePair<string, ParallelMotivationConfig>>.GetEnumerator()
        {
            return this.motivations.GetEnumerator();
        }

        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            foreach (var pair in this.motivations)
            {
                if (pair.Value.AutoName)
                {
                    pair.Value.Name = $"{pair.Key} - ${pair.Value.Name}";
                }
            }

            // Make sure our dependencies are valid
            foreach (var motivation in this.motivations)
            {
                var config = motivation.Value;
                var dependenices = config.Requires.Concat(config.RequiresAny).Concat(config.Until).Concat(config.UntilAny).Concat(config.WhileAny).Concat(config.Blocks).Distinct();
                foreach (var req in dependenices)
                {
                    if (!this.motivations.ContainsKey(req))
                    {
                        throw new SemanticErrorException(motivation.Value.Start, motivation.Value.End, $"Motivation {motivation.Key} \"{motivation.Value.Name}\" requires motivation key {req} which does not exist.");
                    }
                }
            }

            // Do a test run to find circular dependencies.
            // FIXME: This is terrible, check it directly.
            var cache = new Dictionary<string, MotivationStatus>();
            foreach (var key in this.motivations.Keys)
            {
                this.ResolveMotivationStatus(key, GameStateProvider.Empty, cache);
            }
        }

        protected override IEnumerable<IMotivationConfig> GetCurrentMotivations(IGameState state)
        {
            return CacheUtils.Compute(this, nameof(this.GetCurrentMotivations), state, () =>
            {
                var cache = new Dictionary<string, MotivationStatus>();
                return this.motivations.Where(pair => this.ResolveMotivationStatus(pair.Key, state, cache) == MotivationStatus.CanRun).Select(pair => pair.Value).ToArray();
            });
        }

        private MotivationStatus ResolveMotivationStatus(string key, IGameState state, Dictionary<string, MotivationStatus> cache)
        {
            if (!this.motivations.TryGetValue(key, out var motivation))
            {
                throw new ArgumentException(nameof(key), $"Motivation key {key} does not exist.");
            }

            if (cache.TryGetValue(key, out var status))
            {
                if (status == MotivationStatus.Processing)
                {
                    throw new SemanticErrorException(motivation.Start, motivation.End, $"Circular dependency detected in motivation {key} \"{motivation.Name}\".");
                }

                return status;
            }

            if (motivation.IsSatisfied(state))
            {
                cache[key] = MotivationStatus.Satisfied;
            }
            else if (!motivation.IsConditionMet(state))
            {
                // Note: This is different from LinearMotivationCollectionConfig, which will start any motivation provided its previous motivations are satisfied.
                cache[key] = MotivationStatus.MissingRequirements;
            }
            else
            {
                // Mark that we are processing it to catch circular dependencies.
                cache[key] = MotivationStatus.Processing;

                // We could cache blockers per key on after deserialize...
                var blockers = this.motivations.Where(x => x.Value.Blocks.Contains(key)).Select(x => x.Key).ToArray();
                if (blockers.Length > 0 && blockers.Any(x => this.ResolveMotivationStatus(x, state, cache) == MotivationStatus.CanRun))
                {
                    // We are blocked by another motivation that is running.
                    cache[key] = MotivationStatus.MissingRequirements;
                }
                else if (motivation.Requires.Count > 0 && motivation.Requires.Any(x => this.ResolveMotivationStatus(x, state, cache) != MotivationStatus.Satisfied))
                {
                    // At least one req is not satisfied
                    cache[key] = MotivationStatus.MissingRequirements;
                }
                else if (motivation.RequiresAny.Count > 0 && motivation.RequiresAny.All(x => this.ResolveMotivationStatus(x, state, cache) != MotivationStatus.Satisfied))
                {
                    // All RequiresAny are not satisfied
                    cache[key] = MotivationStatus.MissingRequirements;
                }
                else if (motivation.Until.Count > 0 && motivation.Until.All(x => this.ResolveMotivationStatus(x, state, cache) == MotivationStatus.Satisfied))
                {
                    // All Until entries are complete, so we can't run anymore.
                    cache[key] = MotivationStatus.MissingRequirements;
                }
                else if (motivation.UntilAny.Count > 0 && motivation.UntilAny.Any(x => this.ResolveMotivationStatus(x, state, cache) == MotivationStatus.Satisfied))
                {
                    // At least one UntilAny entry is complete, so we can't run anymore.
                    cache[key] = MotivationStatus.MissingRequirements;
                }
                else if (motivation.WhileAny.Count > 0 && motivation.WhileAny.All(x => this.ResolveMotivationStatus(x, state, cache) != MotivationStatus.CanRun))
                {
                    // none of our WhielAny candidates can run, so we cannot run.
                    cache[key] = MotivationStatus.MissingRequirements;
                }
                else
                {
                    cache[key] = MotivationStatus.CanRun;
                }
            }

            return cache[key];
        }

        public class ParallelMotivationConfig : MotivationConfig
        {
            /// <summary>
            /// Gets or sets a value indicating whether this motivation can only run if at least one primary goal can activate.
            /// </summary>
            public bool RequirePrimaryGoals { get; set; } = false;

            /// <summary>
            /// Gets or sets the list of motivations that must be complete before this motivation can run.
            /// All of these motivations must be complete before this motivation can run.
            /// </summary>
            public List<string> Requires { get; set; } = new();

            /// <summary>
            /// Gets or sets the list of motivations for which any completion can allow this motivation to run.
            /// At least one of these motivations must be satisfied for this motivation to run.
            /// </summary>
            public List<string> RequiresAny { get; set; } = new();

            /// <summary>
            /// Gets or sets the list of motivations that must be incomplete for this motivation to run.
            /// This motivation can run until all of these motivations are complete.
            /// </summary>
            public List<string> Until { get; set; } = new();

            /// <summary>
            /// Gets or sets the list of motivations that must be incomplete for this motivation to run.
            /// This motivation can run until any of these motivations are complete.
            /// </summary>
            public List<string> UntilAny { get; set; } = new();

            /// <summary>
            /// Gets or sets the list of motivations that must be running for this motivation to run.
            /// As long as any of these motivations are running, this motivation can run.
            /// </summary>
            public List<string> WhileAny { get; set; } = new();

            /// <summary>
            /// While we are capable of running, block thee other motivations from running
            /// </summary>
            public List<string> Blocks { get; set; } = new();

            public override ConditionResult IsConditionMet(IGameState state)
            {
                var baseCondition = base.IsConditionMet(state);
                if (!baseCondition)
                {
                    return baseCondition;
                }

                if (!this.RequirePrimaryGoals)
                {
                    return ConditionResult.Success;
                }

                // ParallelMotivations with RequirePrimaryGoals wait to activate until at least one primary goal can activate.
                // This is different from LinearMotivationCollectionConfig, which runs motivations as long as the previous motivations are satisfied.
                return CacheUtils.Compute(this, nameof(this.IsConditionMet), state, () =>
                {
                    var failures = new List<ConditionResult>();
                    foreach (var goal in this.PrimaryGoals)
                    {
                        var match = goal.Value.IsConditionMet(state);
                        if (match)
                        {
                            return ConditionResult.Success;
                        }

                        failures.Add(match);
                    }

                    return CompoundConditionResult.ForFailure(failures);
                });
            }
        }
    }
}
