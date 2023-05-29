namespace AutoccultistNS.GameState
{
    using System.Collections.Generic;

    /// <summary>
    /// Gets the game state of the mansus.
    /// </summary>
    public interface IMansusState
    {
        /// <summary>
        /// Gets a value indicating whether the mansus is active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Gets a dictionary mapping deck names to their current card.
        /// </summary>
        IReadOnlyDictionary<string, ICardState> DeckCards { get; }

        /// <summary>
        /// Gets the deck id of the face up deck.
        /// </summary>
        string FaceUpDeck { get; }

        /// <summary>
        /// Gets the face up card.
        /// </summary>
        ICardState FaceUpCard { get; }
    }
}
