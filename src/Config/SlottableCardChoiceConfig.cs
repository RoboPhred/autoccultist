namespace Autoccultist.Config
{
    using Autoccultist.GameState;

    /// <summary>
    /// A config file for specifying cards that are slottable.
    /// </summary>
    public class SlottableCardChoiceConfig : CardChooserConfig, ISlottableCardChoiceConfig
    {
        /// <inheritdoc/>
        protected override bool AdditionalFilter(ICardState card)
        {
            return card.IsSlottable;
        }
    }
}
