namespace Autoccultist.Config.CardChoices
{
    using Autoccultist.Yaml;

    /// <summary>
    /// A card choice in a recipe solution.
    /// </summary>
    [DuckTypeCandidate(typeof(SlottableCardChooserConfig))]
    [DuckTypeCandidate(typeof(MultipleSlottableCardChoiceConfig))]
    public interface ISlottableCardChoiceConfig : ICardChooser
    {
    }
}
