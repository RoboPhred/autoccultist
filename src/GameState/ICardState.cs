namespace AutoccultistNS.GameState
{
    using System.Collections.Generic;
    using SecretHistories.UI;

    /// <summary>
    /// Represents the state of a card in game.
    /// </summary>
    public interface ICardState
    {
        /// <summary>
        /// Gets the element id of the card.
        /// </summary>
        string ElementId { get; }

        /// <summary>
        /// Gets the lifetime remaining on the card.
        /// </summary>
        float LifetimeRemaining { get; }

        /// <summary>
        /// Gets a value indicating whether this card is unique.
        /// </summary>
        bool IsUnique { get; }

        /// <summary>
        /// Gets a value indicating the location of this card.
        /// </summary>
        CardLocation Location { get; }

        /// <summary>
        /// Gets a value indicating whether this card can be slotted.
        /// </summary>
        bool IsSlottable { get; }

        /// <summary>
        /// Gets a dictionary of aspects on this card.
        /// </summary>
        IReadOnlyDictionary<string, int> Aspects { get; }

        /// <summary>
        /// Gets a signature for this card.
        /// </summary>
        /// <remarks>
        /// This is a string that reflects all attributes of this card, and can be used to find if two cards are effectively identical.
        /// </remarks>
        string Signature { get; }

        /// <summary>
        /// Gets the element stack of this card.
        /// </summary>
        /// <returns>An element stack of this singular card.</returns>
        ElementStack ToElementStack();
    }
}
