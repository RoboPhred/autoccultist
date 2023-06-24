namespace AutoccultistNS.Config.CardChoices
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameState;

    /// <summary>
    /// Defines configuration for a list of card choices where the first valid choice will be chosen.
    /// </summary>
    public class MultipleSlottableCardChoiceConfig : ConfigObject, ISlottableCardChoiceConfig
    {
        /// <inheritdoc/>
        public bool Optional { get; set; }

        /// <summary>
        /// Gets or sets a list of slottable card choices to choose from.
        /// </summary>
        public List<SlottableCardChooserConfig> OneOf { get; set; } = new();

        /// <inheritdoc/>
        public IEnumerable<ICardState> SelectChoices(IEnumerable<ICardState> cards)
        {
            var arrayOfCards = cards.ToArray();
            return this.OneOf.SelectMany(c => c.SelectChoices(cards));
        }

        public override string ToString()
        {
            return $"MultipleSlottableCardChoiceConfig({string.Join(", ", this.OneOf.Select(x => x.ToString()))})";
        }
    }
}
