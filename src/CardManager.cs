namespace Autoccultist
{
    using Assets.Core.Interfaces;
    using Autoccultist.GameState;

    /// <summary>
    /// Static classes for dealing with cards in the game.
    /// </summary>
    public static class CardManager
    {
        /// <summary>
        /// Choose a card that satisfies the matcher.
        /// </summary>
        /// <param name="choice">The card matcher to choose a card with.</param>
        /// <returns>The chosen card, or null if none was found.</returns>
        public static IElementStack ChooseCard(ICardChooser choice)
        {
            var state = GameStateProvider.Current;
            return choice.ChooseCard(state.TabletopCards)?.ToElementStack();
        }
    }
}
