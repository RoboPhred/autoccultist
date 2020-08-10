using System.Collections.Generic;

namespace Autoccultist.Brain
{
    public interface IGameState
    {
        bool SituationIsAvailable(string situationId);
        bool CardsCanBeSatisfied(IReadOnlyCollection<ICardMatcher> choices);
    }
}