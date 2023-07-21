namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config.Conditions;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;

    /// <summary>
    /// A Goal represents a collection of impulses which activate under certain conditions,
    /// and continually run until an expected state is reached.
    /// <para>
    /// Goals are made out of multiple impulses, which trigger the actual actions against the game.
    /// </summary>
    [LibraryConfigObject("goals")]
    public class GoalConfig : ImperativeConfigBase, IGoalConfig
    {
        /// <summary>
        /// Gets or sets the condition to determine when this goal is completed.
        /// </summary>
        /// <remarks>
        /// Completed goals will be inactive.
        /// Depending on the imperative parent, goal completions might enable or disable other
        /// sibling imperatives
        /// </remarks>
        public IGameStateConditionConfig CompletedWhen { get; set; }

        /// <summary>
        /// Gets or sets a list of imperatives this goal provides.
        /// </summary>
        public FlatList<IImperativeConfig> Imperatives { get; set; } = new();

        /// <inheritdoc/>
        public override IReadOnlyCollection<IImperative> Children => this.Imperatives;

        /// <inheritdoc/>
        public override ConditionResult IsSatisfied(IGameState state)
        {
            return CacheUtils.Compute(this, nameof(this.IsSatisfied), state, () =>
            {
                if (this.CompletedWhen == null)
                {
                    // Never completes
                    return GeneralConditionResult.ForFailure("Goal has no completion condition.");
                }

                return this.CompletedWhen.IsConditionMet(state);
            });
        }

        public override IEnumerable<string> DescribeCurrentGoals(IGameState gameState)
        {
            return new[] { this.Name };
        }

        public override IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            // For legacy reasons, Goals are active even if their requirements are not met.
            // FIXME: See about removing this
            if (this.IsSatisfied(state))
            {
                return Enumerable.Empty<IImpulse>();
            }

            // Goals tend to be near the top of the imperative stack, so this is a good place for caching.
            // FIXME: We cannot cache this because OperationConfigs rely on Resource constraints, which are not part of this cache.
            // return CacheUtils.Compute(this, nameof(this.GetImpulses), state, () => this.Imperatives.SelectMany(x => x.GetImpulses(state)).ToArray());
            return this.Imperatives.SelectMany(x => x.GetImpulses(state));
        }
    }
}
