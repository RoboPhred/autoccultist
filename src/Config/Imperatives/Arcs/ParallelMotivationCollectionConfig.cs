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

        public ParallelMotivationConfig this[string key] { get => this.motivations[key]; set => this.motivations[key] = value; }

        public override ConditionResult CanActivate(IGameState state)
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

        public override IEnumerable<IImperative> Flatten()
        {
            yield return this;

            foreach (var flat in this.motivations.Values.SelectMany(x => x.Flatten()))
            {
                yield return flat;
            }
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
                foreach (var req in motivation.Value.RequiresAllOf)
                {
                    if (!this.motivations.ContainsKey(req))
                    {
                        throw new SemanticErrorException(motivation.Value.Start, motivation.Value.End, $"Motivation {motivation.Key} \"{motivation.Value.Name}\" requires motivation key {req} which does not exist.");
                    }
                }

                foreach (var until in motivation.Value.UntilAllOf)
                {
                    if (!this.motivations.ContainsKey(until))
                    {
                        throw new SemanticErrorException(motivation.Value.Start, motivation.Value.End, $"Motivation {motivation.Key} \"{motivation.Value.Name}\" waits until motivation key {until} which does not exist.");
                    }
                }

                foreach (var @while in motivation.Value.WhileAnyOf)
                {
                    if (!this.motivations.ContainsKey(@while))
                    {
                        throw new SemanticErrorException(motivation.Value.Start, motivation.Value.End, $"Motivation {motivation.Key} \"{motivation.Value.Name}\" waits while motivation key {@while} which does not exist.");
                    }
                }
            }

            // Do a test run to find circular dependencies.
            var cache = new Dictionary<string, MotivationStatus>();
            foreach (var key in this.motivations.Keys)
            {
                this.ResolveMotivationStatus(key, GameStateProvider.Empty, cache);
            }
        }

        protected override IEnumerable<IMotivationConfig> GetCurrentMotivations(IGameState state)
        {
            var cache = new Dictionary<string, MotivationStatus>();
            return this.motivations.Where(pair => this.ResolveMotivationStatus(pair.Key, state, cache) == MotivationStatus.CanRun).Select(pair => pair.Value);
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
            else if (!motivation.CanActivate(state))
            {
                // Note: This is different from LinearMotivationCollectionConfig, which will start any motivation provided its previous motivations are satisfied.
                cache[key] = MotivationStatus.MissingRequirements;
            }
            else
            {
                // Mark that we are processing it to catch circular dependencies.
                cache[key] = MotivationStatus.Processing;

                if (motivation.RequiresAllOf.Count > 0 && motivation.RequiresAllOf.Any(x => this.ResolveMotivationStatus(x, state, cache) != MotivationStatus.Satisfied))
                {
                    cache[key] = MotivationStatus.MissingRequirements;
                }
                else if (motivation.UntilAllOf.Count > 0 && motivation.UntilAllOf.All(x => this.ResolveMotivationStatus(x, state, cache) == MotivationStatus.Satisfied))
                {
                    cache[key] = MotivationStatus.MissingRequirements;
                }
                else if (motivation.WhileAnyOf.Count > 0 && motivation.WhileAnyOf.All(x => this.ResolveMotivationStatus(x, state, cache) != MotivationStatus.CanRun))
                {
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
            public List<string> RequiresAllOf { get; set; } = new();

            /// <summary>
            /// Gets or sets the list of motivations that must be incomplete for this motivation to run.
            /// This motivation can run until all of these motivations are complete.
            /// </summary>
            public List<string> UntilAllOf { get; set; } = new();

            /// <summary>
            /// Gets or sets the list of motivations that must be running for this motivation to run.
            /// As long as any of these motivations are running, this motivation can run.
            /// </summary>
            public List<string> WhileAnyOf { get; set; } = new();

            public override ConditionResult CanActivate(IGameState state)
            {
                if (!this.RequirePrimaryGoals)
                {
                    return ConditionResult.Success;
                }

                // ParallelMotivations wait to activate until at least one primary goal can activate.
                // This is different from LinearMotivationCollectionConfig, which runs motivations as long as the previous motivations are satisfied.
                var failures = new List<ConditionResult>();
                foreach (var goal in this.PrimaryGoals)
                {
                    var match = goal.Value.CanActivate(state);
                    if (match)
                    {
                        return ConditionResult.Success;
                    }

                    failures.Add(match);
                }

                return new CompoundConditionFailure(failures);
            }
        }
    }
}
