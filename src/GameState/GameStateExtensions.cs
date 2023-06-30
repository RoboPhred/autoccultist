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
    }
}
