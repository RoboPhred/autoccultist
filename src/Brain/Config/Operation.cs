namespace Autoccultist.Brain.Config
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An operation is a series of tasks to complete a verb or situation.
    /// </summary>
    public class Operation : INamedConfigObject, IGameStateCondition
    {
        /// <inheritdoc/>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the situation id to target for this operation.
        /// </summary>
        public string Situation { get; set; }

        /// <summary>
        /// Gets or sets the recipe used to start this situation.
        /// </summary>
        public RecipeSolution StartingRecipe { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of recipe ids to recipe solutions for each ongoing recipe the situation may encounter.
        /// </summary>
        public Dictionary<string, RecipeSolution> OngoingRecipes { get; set; } = new Dictionary<string, RecipeSolution>();

        /// <inheritdoc/>
        public void Validate()
        {
            if (string.IsNullOrEmpty(this.Situation))
            {
                throw new InvalidConfigException($"Operation {this.Name} must have a situation.");
            }
        }

        /// <summary>
        /// Determines if this operation is able to execute given the supplied game state.
        /// </summary>
        /// <param name="state">The game state to match conditions against.</param>
        /// <returns>True if this operation is able to execute at this moment, or False otherwise.</returns>
        public bool IsConditionMet(IGameState state)
        {
            if (!state.IsSituationAvailable(this.Situation))
            {
                return false;
            }

            IEnumerable<CardChoice> requiredCards = new CardChoice[0];

            // We need to ensure all cards are available, including ongoing.

            // TODO: More complex and long running situations may not allow us to have all we need right at the start
            // We should be able to have optional card choices that do not count to starting.
            if (this.StartingRecipe != null)
            {
                requiredCards = requiredCards.Concat(this.StartingRecipe.Slots.Values);
            }

            if (this.OngoingRecipes != null)
            {
                requiredCards = requiredCards.Concat(
                    from ongoing in this.OngoingRecipes.Values
                    from choice in ongoing.Slots.Values
                    select choice);
            }

            return state.CardsCanBeSatisfied(requiredCards.ToArray());
        }
    }
}
