namespace Autoccultist.Brain.Config
{
    using Autoccultist.Brain.Config.Conditions;
    using Autoccultist.GameState;

    /// <summary>
    /// An Imperative represents an action that cannot ever be truly satisfied.
    /// As long as the requirements of the Imperative allow for its execution, the task should execute.
    /// </summary>
    public class Imperative : IConfigObject
    {
        /// <summary>
        /// Gets or sets the human-friendly display name for this imperative.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the priority for this imperative.
        /// Imperatives with a higher priority will run before lower priority imperatives.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Normal;

        /// <summary>
        /// Gets or sets a condition which must be met before this imperative can activate.
        /// </summary>
        public IGameStateConditionConfig Requirements { get; set; }

        /// <summary>
        /// Gets or sets a condition on which to prevent this imperative from activating.
        /// </summary>
        public IGameStateConditionConfig Forbidders { get; set; }

        /// <summary>
        /// Gets or sets the operation to perform when this imperative is triggered.
        /// </summary>
        public Operation Operation { get; set; }

        /// <inheritdoc/>
        public void Validate()
        {
            if (this.Operation == null)
            {
                throw new InvalidConfigException($"Imperative {this.Name} must have an operation.");
            }

            this.Requirements?.Validate();

            this.Forbidders?.Validate();

            this.Operation.Validate();
        }

        /// <summary>
        /// Determines whether the imperative can execute given the supplied game state.
        /// </summary>
        /// <param name="state">The game state to check conditions against.</param>
        /// <returns>True if this imperative can execute right away, or False if it cannot.</returns>
        public bool CanExecute(IGameState state)
        {
            // Optionally check required cards for starting the imperative
            if (this.Requirements?.IsConditionMet(state) == false)
            {
                return false;
            }

            // Sometimes, we want to stop an imperative if other cards are present.
            if (this.Forbidders?.IsConditionMet(state) == true)
            {
                return false;
            }

            if (!this.Operation.IsConditionMet(state))
            {
                return false;
            }

            return true;
        }
    }
}
