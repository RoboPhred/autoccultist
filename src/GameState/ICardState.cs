namespace Autoccultist.GameState
{
    using System.Collections.Generic;
    using Assets.Core.Interfaces;

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
        /// Gets a dictionary of aspects on this card.
        /// </summary>
        IReadOnlyDictionary<string, int> Aspects { get; }

        // FIXME: Temporary.  We need to have a consumption system which removes the card from the state list.
        //  as it is, we can keep calling ToElementStack forever and consume all cards of this type without ever touching any other card state

        /// <summary>
        /// Gets the element stack of this card.
        /// </summary>
        /// <returns>An element stack of this singular card.</returns>
        IElementStack ToElementStack();
    }
}
