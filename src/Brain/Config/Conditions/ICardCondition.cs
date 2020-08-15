using System.Collections.Generic;
using Autoccultist.Yaml;

namespace Autoccultist.Brain.Config.Conditions
{
    [DuckTypeCandidate(typeof(CardSetCondition))]
    [DuckTypeCandidate(typeof(CardChoice))]
    public interface ICardCondition : IGameStateConditionConfig
    {
        List<CardChoice> CardSet { get; }
    }
}
