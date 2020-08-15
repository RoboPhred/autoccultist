namespace Autoccultist
{
    using System.Collections.Generic;
    using Autoccultist.GameState;

    /// <summary>
    /// Describes a class that can determine if a card matches its specifications.
    /// </summary>
    public interface ICardChooser
    {
        /// <summary>
        /// Choose a card from the given candidates.
        /// </summary>
        /// <param name="cards">An enumerable of candidate cards to choose from.</param>
        /// <returns>The chosen card, or null if none are chosen.</returns>
        ICardState ChooseCard(IEnumerable<ICardState> cards);
    }
}
