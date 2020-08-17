namespace Autoccultist.Brain
{
    using System.Collections.Generic;
    using Autoccultist.Brain.Config.Conditions;
    using Autoccultist.GameState;

    /// <summary>
    /// A solution to a situation recipe.
    /// </summary>
    public class RecipeSolution : IGameStateCondition
    {
        /// <summary>
        /// Gets or sets a dictionary of slot ids to card choices for those slots.
        /// </summary>
        public IDictionary<string, CardChoice> Slots { get; set; } = new Dictionary<string, CardChoice>();

        /// <summary>
        /// Determines if this recipe solution is able to be resolved given the supplied game state.
        /// </summary>
        /// <param name="state">The game state to check availability against.</param>
        /// <returns>True if this recipe solution can be performed in the current game state, or False otherwise.</returns>
        public bool IsConditionMet(IGameState state)
        {
            return state.CardsCanBeSatisfied(this.Slots.Values);
        }
    }
}
