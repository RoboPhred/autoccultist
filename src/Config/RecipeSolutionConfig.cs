namespace Autoccultist.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using Assets.CS.TabletopUI;
    using Autoccultist.Brain;
    using Autoccultist.GameState;

    /// <summary>
    /// Configuration for a solution to a situation recipe.
    /// </summary>
    public class RecipeSolutionConfig : IRecipeSolution
    {
        /// <summary>
        /// Gets or sets a dictionary of slot names to card choices.
        /// </summary>
        public Dictionary<string, RecipeCardChoiceConfig> Slots { get; set; } = new Dictionary<string, RecipeCardChoiceConfig>();

        /// <summary>
        /// Gets or sets a solver for a mansus event triggered by this recipe.
        /// </summary>
        public MansusSolutionConfig MansusChoice { get; set; }

        /// <inheritdoc/>
        IMansusSolution IRecipeSolution.MansusChoice => this.MansusChoice;

        /// <inheritdoc/>
        public bool IsConditionMet(IGameState state)
        {
            // TODO: Optional card slots.
            var desiredCards =
                from choice in this.Slots.Values
                select choice;
            return state.CardsCanBeSatisfied(desiredCards);
        }

        /// <inheritdoc/>
        public ICardChooser ResolveSlotCard(RecipeSlot slot)
        {
            if (!this.Slots.TryGetValue(slot.GoverningSlotSpecification.Id, out var choice))
            {
                return null;
            }

            return choice;
        }
    }
}
