namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config.Conditions;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// A Goal represents a collection of impulses which activate under certain conditions,
    /// and continually run until an expected state is reached.
    /// <para>
    /// Goals are made out of multiple impulses, which trigger the actual actions against the game.
    /// </summary>
    public class GoalConfig : INamedConfigObject, IGoal, IAfterYamlDeserialization
    {
        /// <inheritdoc/>
        public string Name { get; set; }

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
        /// Gets or sets a list of bundles of impulses to also include in this goal.
        /// <para>
        /// These impulses will be lower priority than the goal's normal impulses, unless overriden with an impulse's <see cref="ImpulseConfig.Priority"/> property.
        /// </summary>
        public List<List<ImpulseConfig>> ImpulseSets { get; set; } = new List<List<ImpulseConfig>>();

        /// <summary>
        /// Gets or sets a list of impulses this goal provides.
        /// <para>
        /// Each impulse provides an operation and conditions under which the operation will be performed.
        /// </summary>
        public List<ImpulseConfig> Impulses { get; set; } = new List<ImpulseConfig>();

        /// <inheritdoc/>
        string IGoal.Name => this.Name;

        /// <inheritdoc/>
        IGameStateCondition IGoal.Requirements => this.Requirements;

        /// <inheritdoc/>
        IGameStateCondition IGoal.CompletedWhen => this.CompletedWhen;

        /// <inheritdoc/>
        IReadOnlyList<IImpulse> IGoal.Impulses => this.Impulses.Concat(this.ImpulseSets.SelectMany(set => set)).ToArray();

        /// <summary>
        /// Determines whether the goal can activate with the given game state.
        /// </summary>
        /// <param name="state">The game state to check conditions against.</param>
        /// <returns>True if this goal is able to activate, False otherwise.</returns>
        public bool CanActivate(IGameState state)
        {
            if (this.IsSatisfied(state))
            {
                return false;
            }

            if (this.Requirements?.IsConditionMet(state) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether this goal is completed with the given game state.
        /// </summary>
        /// <param name="state">The game state to check conditions against.</param>
        /// <returns>True if the goal is completed, False otherwise.</returns>
        public bool IsSatisfied(IGameState state)
        {
            return this.CompletedWhen?.IsConditionMet(state) == true;
        }

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            // Used to require goals to have impulses, but we can have goals that wait for events.
        }
    }
}
