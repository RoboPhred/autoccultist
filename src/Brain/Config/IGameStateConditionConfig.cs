using Autoccultist.Yaml;

namespace Autoccultist.Brain.Config
{
    [DuckTypeCandidate(typeof(CompoundCondition))]
    [DuckTypeCandidate(typeof(SituationCondition))]
    [DuckTypeCandidate(typeof(CardSetCondition))]
    [DuckTypeCandidate(typeof(CardChoice))]
    public interface IGameStateConditionConfig
    {
        bool IsConditionMet(IGameState state);
        void Validate();
    }
}