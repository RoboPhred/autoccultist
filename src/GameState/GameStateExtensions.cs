namespace Autoccultist.GameState
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A set of extensions for <see cref="IGameState"/>.
    /// </summary>
    public static class GameStateExtensions
    {
        /// <summary>
        /// Determines if the given situation is available for use.
        /// </summary>
        /// <param name="state">The game state to check.</param>
        /// <param name="situationId">The situation id of the situation to check.</param>
        /// <returns>True if the situation is available for use, or False if it is otherwise occupied.</returns>
        public static bool IsSituationAvailable(this IGameState state, string situationId)
        {
            var situation = state.Situations.FirstOrDefault(s => s.SituationId == situationId);
            if (situation == null)
            {
                return false;
            }

            return !situation.IsOccupied;
        }

        /// <summary>
        /// Determines if all card matchers can satisfy their cards.
        /// This takes into account card matchers wanting to consume their cards, so once a matcher
        /// chooses a card, other matchers may not consider that card for their choice.
        /// </summary>
        /// <param name="state">The game state to check.</param>
        /// <param name="choosers">A collection of card matchers to check cards against.</param>
        /// <returns>True if all card matchers can satisfy their matches, or False otherwise.</returns>
        public static bool CardsCanBeSatisfied(this IGameState state, IEnumerable<ICardChooser> choosers)
        {
            // MissingMethodException ???
            // var remainingCards = state.TabletopCards.ToHashSet();
            var remainingCards = new HashSet<ICardState>(state.TabletopCards);
            foreach (var chooser in choosers)
            {
                var choice = chooser.ChooseCard(remainingCards);
                if (choice == null)
                {
                    return false;
                }

                remainingCards.Remove(choice);
            }

            return true;
        }
    }
}
