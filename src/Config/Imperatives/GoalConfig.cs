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
    public class GoalConfig : NamedConfigObject, IImperativeConfig, IGoal
    {
        /// <summary>
        /// Gets or sets the condition which is required to be met for this goal to activate.
        /// <para>
        /// The goal will remain activated after these conditions are met,
        /// and continue operating until the CompletedWhen condition is met.
        /// </summary>
        public IGameStateConditionConfig Requirements { get; set; }

        /// <summary>
        /// Gets or sets the condition to determine when this goal is completed.
        /// <para>
        /// Once started, the goal will continue to operate until its completion conditions are met.
        /// </summary>
        public IGameStateConditionConfig CompletedWhen { get; set; }

        /// <summary>
        /// Gets or sets a list of imperatives this goal provides.
        /// </summary>
        public FlatList<IImperativeConfig> Imperatives { get; set; } = new();

        /// <inheritdoc/>
        IReadOnlyCollection<IImperative> IImperative.Children => this.Imperatives;

        /// <inheritdoc/>
        public ConditionResult CanActivate(IGameState state)
        {
            var satsifiedMatch = this.IsSatisfied(state);
            if (satsifiedMatch)
            {
                return AddendedConditionResult.Addend(GameStateConditionResult.ForFailure(this.CompletedWhen, satsifiedMatch), "Goal is already completed.");
            }

            if (this.Requirements != null)
            {
                var requirementsMatch = this.Requirements.IsConditionMet(state);
                if (!requirementsMatch)
                {
                    return AddendedConditionResult.Addend(GameStateConditionResult.ForFailure(this.Requirements, requirementsMatch), "Goal requirements are not met.");
                }
            }

            return ConditionResult.Success;
        }

        /// <inheritdoc/>
        public ConditionResult IsSatisfied(IGameState state)
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

        public IEnumerable<string> DescribeCurrentGoals(IGameState gameState)
        {
            return new[] { this.Name };
        }

        public IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            return this.Imperatives.SelectMany(x => x.GetImpulses(state));
        }
    }
}
