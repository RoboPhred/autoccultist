namespace AutoccultistNS.Config.Conditions
{
    using AutoccultistNS.Yaml;

    /// <summary>
    /// Defines a config node that checks for a game state condition.
    /// </summary>
    [DuckTypeCandidate(typeof(CompoundCondition))]
    [DuckTypeCandidate(typeof(SituationCondition))]
    [DuckTypeCandidate(typeof(CardSetCondition))]
    [DuckTypeCandidate(typeof(CardExistsCondition))]
    [DuckTypeCandidate(typeof(MemoryCondition))]
    public interface IGameStateConditionConfig : INamedConfigObject, IGameStateCondition
    {
    }
}
