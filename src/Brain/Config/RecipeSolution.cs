namespace Autoccultist.Brain.Config
{
    using System.Collections.Generic;
    using System.Linq;

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
            var cardMatchers = this.Slots.Values.Cast<ICardMatcher>().ToArray();
            return state.CardsCanBeSatisfied(cardMatchers);
        }
    }
}
