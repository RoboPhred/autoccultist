namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config.CardChoices;

    /// <summary>
    /// Configuration for a solution to a situation recipe.
    /// </summary>
    public class RecipeSolutionConfig : NamedConfigObject, IRecipeSolution
    {
        /// <summary>
        /// A cached read only dictionary of slot names to card choices.
        /// </summary>
        private IReadOnlyDictionary<string, ISlotCardChooser> slotSolutions;

        /// <summary>
        /// Gets or sets a value indicating whether this recipe requires its cards to start the operation.
        /// </summary>
        public bool RequireSlotCards { get; set; } = true;

        /// <summary>
        /// Gets or sets the required cards to start this recipe.
        /// These cards will be required in addition to any slot card requirements, if RequireSlotCards is set.
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

        /// <summary>
        /// Gets or sets a value indicating whether this recipe should be re-run if a card is stolen from us while we're working on it.
        /// </summary>
        public bool RerunOnTheft { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this recipe should end the operation.
        /// This leaves the situation open for other operations to target it.
        /// </summary>
        public bool EndOperation { get; set; } = false;

        /// <inheritdoc/>
        IReadOnlyDictionary<string, ISlotCardChooser> IRecipeSolution.SlotSolutions
        {
            get
            {
                if (this.slotSolutions == null)
                {
                    this.slotSolutions = this.Slots.ToDictionary(x => x.Key, x => x.Value as ISlotCardChooser);
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
            return explicitRequirements.Concat(implicitRequirements).Where(x => !x.Optional).ToArray();
        }

        public override string ToString()
        {
            return $"RecipeSolution(Name = {this.Name})";
        }
    }
}
