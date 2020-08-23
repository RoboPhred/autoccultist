namespace Autoccultist.Brain
{
    using Assets.CS.TabletopUI;

    /// <summary>
    /// A solution to a situation recipe.
    /// </summary>
    public interface IRecipeSolution : IGameStateCondition
    {
        /// <summary>
        /// Gets the solution for the mansus choice of this recipe, if any.
        /// </summary>
        IMansusSolution MansusChoice { get; }

        /// <summary>
        /// Gets the card for the given slot.
        /// </summary>
        /// <param name="slot">The slot to fill.</param>
        /// <returns>The card state of the card chosen, or null if the slot should remain empty.</returns>
        // FIXME: Should receive IGameState and return a IConsumedCard
        ICardChooser ResolveSlotCard(RecipeSlot slot);
    }
}
