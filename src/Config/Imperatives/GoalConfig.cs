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
        /// Gets or sets a list of impulses this goal provides.
        /// <para>
        /// Each impulse provides an operation and conditions under which the operation will be performed.
        /// </summary>
        public FlatList<IImpulseConfig> Impulses { get; set; } = new();

        /// <summary>
        /// Determines whether the goal can activate with the given game state.
        /// </summary>
        /// <param name="state">The game state to check conditions against.</param>
        /// <returns>True if this goal is able to activate, False otherwise.</returns>
        public ConditionResult CanActivate(IGameState state)
        {
            var satsifiedMatch = this.IsSatisfied(state);
            if (satsifiedMatch)
            {
                return new AddendedConditionFailure(new GameStateConditionFailure(this.CompletedWhen, satsifiedMatch), "Goal is already completed.");
            }

            if (this.Requirements != null)
            {
                var requirementsMatch = this.Requirements.IsConditionMet(state);
                if (!requirementsMatch)
                {
                    return new AddendedConditionFailure(new GameStateConditionFailure(this.Requirements, requirementsMatch), "Goal requirements are not met.");
                }
            }

            return ConditionResult.Success;
        }

        /// <summary>
        /// Determines whether this goal is completed with the given game state.
        /// </summary>
        /// <param name="state">The game state to check conditions against.</param>
        /// <returns>True if the goal is completed, False otherwise.</returns>
        public ConditionResult IsSatisfied(IGameState state)
        {
            if (this.CompletedWhen == null)
            {
                // Never completes
                return new GeneralConditionFailure("Goal has no completion condition.");
            }

            return this.CompletedWhen.IsConditionMet(state);
        }

        public IEnumerable<string> DescribeCurrentGoals(IGameState gameState)
        {
            return new[] { this.Name };
        }

        public IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            return this.Impulses;
        }

        public IEnumerable<IImperative> Flatten()
        {
            return new IImperative[] { this };
        }
    }
}
