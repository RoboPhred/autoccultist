using Autoccultist.Yaml;

namespace Autoccultist.Brain.Config
{
    [DuckTypeCandidate(typeof(CompoundCondition))]
    [DuckTypeCandidate(typeof(CardSetCondition))]
    [DuckTypeCandidate(typeof(CardChoice))]
    public interface IGameStateCondition
    {
        bool IsConditionMet(IGameState state);
    }
}