namespace Autoccultist.Config
{
    /// <summary>
    /// A card choice in a recipe solution.
    /// </summary>
    public class RecipeCardChoiceConfig : CardChoiceConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether this card is optional in the recipe.
        /// </summary>
        // TODO: Waiting on IRecipeSolution.ResolveSlotCard returning something other than ICardChooser
        // public bool Optional { get; set; } = false;
    }
}
