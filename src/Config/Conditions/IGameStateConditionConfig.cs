namespace Autoccultist.Config.Conditions
{
    using Autoccultist.Yaml;

    /// <summary>
    /// Defines a config node that checks for a game state condition.
    /// </summary>
    [DuckTypeCandidate(typeof(CompoundCondition))]
    [DuckTypeCandidate(typeof(SituationCondition))]
    [DuckTypeCandidate(typeof(CardSetCondition))]
    [DuckTypeCandidate(typeof(CardChoiceCondition))]
    public interface IGameStateConditionConfig : IConfigObject, IGameStateCondition
    {
    }
}
