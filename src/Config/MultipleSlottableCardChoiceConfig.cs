namespace Autoccultist.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using Autoccultist.GameState;

    /// <summary>
    /// Defines configuration for a list of card choices where the first valid choice will be chosen.
    /// </summary>
    public class MultipleSlottableCardChoiceConfig : ISlottableCardChoiceConfig, IConfigObject
    {
        /// <summary>
        /// Gets pr sets a list of slottable card choices to choose from.
        /// </summary>
        public List<SlottableCardChoiceConfig> OneOf { get; set; } = new();

        /// <inheritdoc/>
        public ICardState ChooseCard(IEnumerable<ICardState> cards)
        {
            var arrayOfCards = cards.ToArray();
            return this.OneOf.Select(c => c.ChooseCard(cards)).FirstOrDefault(c => c != null);
        }
    }
}
