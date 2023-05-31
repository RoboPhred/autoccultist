namespace AutoccultistNS.Config.CardChoices
{
    using AutoccultistNS.GameState;

    /// <summary>
    /// A config file for specifying cards that are slottable.
    /// </summary>
    public class SlottableCardChooserConfig : CardChooserConfig, ISlottableCardChoiceConfig
    {
        /// <inheritdoc/>
        protected override bool AdditionalFilter(ICardState card)
        {
            return card.IsSlottable;
        }
    }
}
