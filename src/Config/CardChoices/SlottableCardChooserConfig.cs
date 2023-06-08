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
            // Note: We are now treating cards in verbs that have not consumed them yet to still be slottale.
            // This is in preperation to let cards be stolen by other operations, as an advanced technique
            // to pause the decay timer on cards (see: sulochana in talk verb with influences)
            return card.Location == CardLocation.Tabletop && card.IsSlottable;
        }
    }
}
