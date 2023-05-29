namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config.CardChoices;

    /// <summary>
    /// Configuration for a solution to a situation recipe.
    /// </summary>
    public class RecipeSolutionConfig : IRecipeSolution
    {
        /// <summary>
        /// A cached read only dictionary of slot names to card choices.
        /// </summary>
        private IReadOnlyDictionary<string, ICardChooser> slotSolutions;

        /// <summary>
        /// Gets or sets a value indicating whether this recipe requires its cards to start the operation.  This only applies to ongoing recipes.
        /// </summary>
        public bool RequireSlotCards { get; set; } = true;

        /// <summary>
        /// Gets or sets the required cards to start this operation.
        /// If not specified, the required cards will be assumed from the slots.
        /// </summary>
        public List<ISlottableCardChoiceConfig> CardRequirements { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of slot names to card choices.
        /// </summary>
        public Dictionary<string, ISlottableCardChoiceConfig> Slots { get; set; } = new();

        /// <summary>
        /// Gets or sets a solver for a mansus event triggered by this recipe.
        /// </summary>
        public MansusSolutionConfig MansusChoice { get; set; }

        /// <inheritdoc/>
        IReadOnlyDictionary<string, ICardChooser> IRecipeSolution.SlotSolutions
        {
            get
            {
                if (this.slotSolutions == null)
                {
                    this.slotSolutions = this.Slots.ToDictionary(x => x.Key, x => x.Value as ICardChooser);
                }

                return this.slotSolutions;
            }
        }

        /// <inheritdoc/>
        IMansusSolution IRecipeSolution.MansusChoice => this.MansusChoice;

        /// <inheritdoc/>
        public IEnumerable<ICardChooser> GetRequiredCards()
        {
            var explicitRequirements = (IEnumerable<ISlottableCardChoiceConfig>)this.CardRequirements ?? new ISlottableCardChoiceConfig[0];
            var implicitRequirements = this.RequireSlotCards ? this.Slots.Values.Select(x => x) : new ISlottableCardChoiceConfig[0];
            return explicitRequirements.Concat(implicitRequirements).ToArray();
        }
    }
}
