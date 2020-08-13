using System.Collections.Generic;
using Autoccultist.GameState;

namespace Autoccultist.Brain.Config
{
    public class RecipeSolution
    {
        // TODO: Choices should be able to be made optional
        public IDictionary<string, CardChoice> Slots;

        // TODO: OperationOrchestration should use this directly.
        public bool TryConsumeCards(IGameState state, out IReadOnlyDictionary<string, IConsumedToken> choices)
        {
            choices = null;


            var result = new Dictionary<string, IConsumedToken>();
            foreach (var entry in this.Slots)
            {
                if (!entry.Value.TryConsume(state, out var token))
                {
                    return false;
                }
                result.Add(entry.Key, token);
            }

            choices = result;
            return true;
        }
    }
}