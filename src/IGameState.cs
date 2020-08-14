using System.Collections.Generic;

namespace Autoccultist
{
    // TODO: We can probably get rid of this now that CardManager and SituationSolutionRunner are static
    public interface IGameState
    {
        bool IsSituationAvailable(string situationId);
        bool CardsCanBeSatisfied(IReadOnlyCollection<ICardMatcher> choices);
    }
}