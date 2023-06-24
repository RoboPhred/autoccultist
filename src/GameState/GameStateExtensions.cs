namespace AutoccultistNS.GameState
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A set of extensions for <see cref="IGameState"/>.
    /// </summary>
    public static class GameStateExtensions
    {
        /// <summary>
        /// Gets an enumerable of all cards in the game.
        /// </summary>
        /// <returns>An enumerable of all cards in the game.</returns>
        public static IEnumerable<ICardState> GetAllCards(this IGameState state)
        {
            var allCards = state.TabletopCards.Concat(state.EnRouteCards).Concat(state.Situations.SelectMany(s => s.StoredCards.Concat(s.GetSlottedCards()).Concat(s.OutputCards))).Concat(state.CodexCards);
            switch (state.Mansus.State)
            {
                case PortalActiveState.AwaitingCollection:
                    allCards = allCards.Concat(new[] { state.Mansus.OutputCard });
                    break;
                case PortalActiveState.AwaitingSelection:
                    allCards = allCards.Concat(state.Mansus.DeckCards.Values);
                    break;
            }

            return allCards;
        }

        /// <summary>
        /// Determines if all card matchers can satisfy their cards.
        /// Each card chooser to find a choice will remove that card from the pool that other choices can choose from.
        /// </summary>
        /// <param name="state">The game state to check.</param>
        /// <param name="choosers">A collection of card matchers to check cards against.</param>
        /// <param name="unsatisfiedChoice">The card matcher that could not be satisfied, or null if all matchers were satisfied.</param>
        /// <returns>True if all card matchers can satisfy their matches, or False otherwise.</returns>
        public static bool CardsCanBeSatisfied(this IGameState state, IEnumerable<ICardChooser> choosers, out ICardChooser unsatisfiedChoice)
        {
            return choosers.ChooseAll(state.GetAllCards(), out unsatisfiedChoice) != null;
        }
    }
}
