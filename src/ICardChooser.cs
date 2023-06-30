namespace AutoccultistNS
{
    using System.Collections.Generic;
    using AutoccultistNS.GameState;

    /// <summary>
    /// Describes a class that can determine if a card matches its specifications.
    /// </summary>
    public interface ICardChooser
    {
        /// <summary>
        /// Choose a card from the given candidates.
        /// </summary>
        /// <param name="cards">An enumerable of candidate cards to choose from.</param>
        /// <param name="state">The current game state.</param>
        /// <returns>An enumerable of card that can be chosen in order of priority.</returns>
        IEnumerable<ICardState> SelectChoices(IEnumerable<ICardState> cards, IGameState state);
    }
}
