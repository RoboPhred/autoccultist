namespace AutoccultistNS.Config.CardChoices
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameState;

    /// <summary>
    /// Defines configuration for a list of card choices where the first valid choice will be chosen.
    /// </summary>
    public class MultipleSlottableCardChoiceConfig : ISlottableCardChoiceConfig, IConfigObject
    {
        /// <inheritdoc/>
        public bool Optional { get; set; }

        /// <summary>
        /// Gets or sets a list of slottable card choices to choose from.
        /// </summary>
        public List<SlottableCardChooserConfig> OneOf { get; set; } = new();

        /// <inheritdoc/>
        public ICardState ChooseCard(IEnumerable<ICardState> cards)
        {
            var arrayOfCards = cards.ToArray();
            return this.OneOf.Select(c => c.ChooseCard(cards)).FirstOrDefault(c => c != null);
        }

        public override string ToString()
        {
            return $"MultipleSlottableCardChoiceConfig({string.Join(", ", this.OneOf.Select(x => x.ToString()))})";
        }
    }
}
