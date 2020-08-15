namespace Autoccultist.Brain.Config
{
    using System.Collections.Generic;
    using Autoccultist.Brain.Config.Conditions;
    using Autoccultist.GameState;

    /// <summary>
    /// A Goal represents a collection of imperatives which activate under certain conditions,
    /// and continually run until an expected state is reached.
    /// <para>
    /// Goals are made out of multiple imperatives, which trigger the actual actions against the game.
    /// </summary>
    public class Goal : INamedConfigObject
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
        /// Gets or sets a list of imperatives this goal provides.
        /// <para>
        /// Each imperative provides an operation and conditions under which the operation will be performed.
        /// </summary>
        public List<Imperative> Imperatives { get; set; } = new List<Imperative>();

        /// <inheritdoc/>
        public void Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new InvalidConfigException("Goal must have a name.");
            }

            if (this.Imperatives == null || this.Imperatives.Count == 0)
            {
                throw new InvalidConfigException("Goal must have an imperative");
            }

            foreach (var imperative in this.Imperatives)
            {
                imperative.Validate();
            }
        }

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
    }
}
