namespace Autoccultist.GameState
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the state of a situation.
    /// </summary>
    public interface ISituationState
    {
        /// <summary>
        /// Gets the id of the situation.
        /// </summary>
        string SituationId { get; }

        /// <summary>
        /// Gets a value indicating whether the situation is busy performing a verb.
        /// </summary>
        bool IsOccupied { get; }

        /// <summary>
        /// Gets the current recipe this situation is working on, if any.
        /// </summary>
        string CurrentRecipe { get; }

        /// <summary>
        /// Gets the time remaining on the current recipe, if any.
        /// </summary>
        float? RecipeTimeRemaining { get; }

        /// <summary>
        /// Gets a collection of cards stored inside this situation.
        /// </summary>
        IReadOnlyCollection<ICardState> StoredCards { get; }

        /// <summary>
        /// Gets a collection of slotted cards in this situation.
        /// Typically, these are ongoing slots.
        /// </summary>
        IReadOnlyCollection<ICardState> SlottedCards { get; }
    }
}
