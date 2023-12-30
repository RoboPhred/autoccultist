namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Serialization;

    [CustomDeserializer(typeof(MotivationCollectionConfigDeserializer))]
    public abstract class MotivationCollectionConfig : NamedConfigObject, IImperativeConfig
    {
        [YamlIgnore]
        public UISettingsConfig UI { get; set; } = new UISettingsConfig { Visible = false };

        /// <summary>
        /// Gets the total number of motivations in this config
        /// </summary>
        public abstract int Count { get; }

        /// <inheritdoc/>
        public abstract IReadOnlyCollection<IImperative> Children { get; }

        /// <inheritdoc/>
        public abstract ConditionResult IsConditionMet(IGameState state);

        /// <inheritdoc/>
        public ConditionResult IsSatisfied(IGameState state)
        {
            return CacheUtils.Compute(this, nameof(this.IsSatisfied), state, () =>
            {
                var motivations = this.GetCurrentMotivations(state);

                foreach (var motivation in motivations)
                {
                    var result = motivation.IsSatisfied(state);
                    if (!result)
                    {
                        return AddendedConditionResult.Addend(result, "Active motivation is not satisfied");
                    }
                }

                return ConditionResult.Success;
            });
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            return CacheUtils.Compute(this, nameof(this.DescribeCurrentGoals), state, () =>
            {
                var motivations = this.GetCurrentMotivations(state);
                return motivations.SelectMany(x => x.DescribeCurrentGoals(state)).ToArray();
            });
        }

        public virtual IEnumerable<IImperative> GetActiveChildren(IGameState state)
        {
            return this.GetCurrentMotivations(state);
        }

        /// <inheritdoc/>
        public IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            if (!this.IsConditionMet(state) || this.IsSatisfied(state))
            {
                return Enumerable.Empty<IImpulse>();
            }

            // Note: Each motivation has a GetImpulses too, but we want to rearrange the primary and supporting goals.
            // For legacy reasons, we ignore goal.IsConditionMet
            // FIXME: This would be a prime candidate for caching, except that some conditions rely explicitly on Resources in use,
            // which are not reflected in the game state.  See TODO
            var impulses = from motivation in this.GetCurrentMotivations(state)
                           let primaryGoals = motivation.PrimaryGoals.Select(g => (g, 1))
                           let secondaryGoals = motivation.SupportingGoals.Select(g => (g, 0))
                           let goals = primaryGoals.Concat(secondaryGoals).OrderByDescending(x => x.Item2).Select(x => x.Item1)
                           from goal in goals
                           where !goal.IsSatisfied(state)
                           from impulse in goal.GetImpulses(state)
                           select impulse;

            return impulses;
        }

        /// <summary>
        /// Gets the current active motivations from the game state.
        /// </summary>
        protected abstract IEnumerable<IMotivationConfig> GetCurrentMotivations(IGameState state);
    }
}
