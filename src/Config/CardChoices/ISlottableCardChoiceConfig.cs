namespace AutoccultistNS.Config.CardChoices
{
    using AutoccultistNS.Yaml;

    /// <summary>
    /// A card choice in a recipe solution.
    /// </summary>
    [DuckTypeCandidate(typeof(SlottableCardChooserConfig))]
    [DuckTypeCandidate(typeof(MultipleSlottableCardChoiceConfig))]
    public interface ISlottableCardChoiceConfig : ICardChooser
    {
    }
}
