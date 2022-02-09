namespace Autoccultist.Config
{
    using Autoccultist.Yaml;

    /// <summary>
    /// A card choice in a recipe solution.
    /// </summary>
    [DuckTypeCandidate(typeof(SlottableCardChoiceConfig))]
    [DuckTypeCandidate(typeof(MultipleSlottableCardChoiceConfig))]
    public interface ISlottableCardChoiceConfig : ICardChooser
    {
    }
}
