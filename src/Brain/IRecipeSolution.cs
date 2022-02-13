namespace Autoccultist.Brain
{
    using System.Collections.Generic;
    using Assets.CS.TabletopUI;

    /// <summary>
    /// A solution to a situation recipe.
    /// </summary>
    public interface IRecipeSolution
    {
        /// <summary>
        /// Gets the solution for the mansus choice of this recipe, if any.
        /// </summary>
        IMansusSolution MansusChoice { get; }

        /// <summary>
        /// Gets a collection of card choices that must all be satisfied for this recipe solution to start.
        /// </summary>
        /// <returns>A collection of card choices that must be satisified to start this recipe solution.</returns>
        IEnumerable<ICardChooser> GetRequiredCards();

        /// <summary>
        /// /// Gets the card for the given slot.
        /// </summary>
        /// <param name="slot">The slot to fill.</param>
        /// <returns>The card state of the card chosen, or null if the slot should remain empty.</returns>
        // FIXME: Should receive IGameState and return a IConsumedCard
        ICardChooser ResolveSlotCard(RecipeSlot slot);
    }
}
