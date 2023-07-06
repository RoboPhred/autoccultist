namespace AutoccultistNS.GameState
{
    using System.Collections.Generic;
    using SecretHistories.Enums;

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
        /// Gets the current state of the situation.
        /// </summary>
        StateEnum State { get; }

        /// <summary>
        /// Gets a value indicating whether the situation is busy performing a recipe.
        /// </summary>
        bool IsOccupied { get; }

        /// <summary>
        /// Gets the current recipe this situation is working on, if any.
        /// </summary>
        string CurrentRecipe { get; }

        /// <summary>
        /// Gets a value indicating whether the current recipe is a Mansus recipe.
        /// </summary>
        string CurrentRecipePortal { get; }

        /// <summary>
        /// Gets the time remaining on the current recipe, if any.
        /// </summary>
        float? RecipeTimeRemaining { get; }

        /// <summary>
        /// Gets the slots for the current recipe, if any.
        /// </summary>
        IReadOnlyCollection<ISituationSlot> RecipeSlots { get; }

        /// <summary>
        /// Gets a collection of cards stored inside this situation.
        /// </summary>
        IReadOnlyCollection<ICardState> StoredCards { get; }

        /// <summary>
        /// Gets a collection of cards pending output in the situation.
        /// </summary>
        IReadOnlyCollection<ICardState> OutputCards { get; }
    }
}
