namespace AutoccultistNS.GameState
{
    using System.Collections.Generic;

    /// <summary>
    /// Gets the game state of the mansus.
    /// </summary>
    public interface IPortalState
    {
        /// <summary>
        /// Gets the state of the portal.
        /// </summary>
        PortalActiveState State { get; }

        /// <summary>
        /// Gets the id of the portal, or null if none is open.
        /// </summary>
        string PortalId { get; }

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

        /// <summary>
        /// Gets the card sitting in the mansus output sphere.
        /// </summary>
        ICardState OutputCard { get; }
    }
}
