using System.Collections.Generic;
using System.Linq;

namespace Autoccultist.GameState
{
    public static class GameStateExtensions
    {
        public static IEnumerable<TableCardState> GetTableCards(this IGameState state)
        {
            return state.TabledItems.OfType<TableCardState>();
        }

        public static IEnumerable<SituationState> GetSituations(this IGameState state)
        {
            return state.TabledItems.OfType<SituationState>();
        }

        public static SituationState GetSituation(this IGameState state, string situationId)
        {
            return state.GetSituations().FirstOrDefault(x => x.TokenId == situationId);
        }

        public static IGameState CreateConsumptionScope(this IGameState state)
        {
            // TODO: Return a child gamestate which mantains its own list of consumed tokens,
            //  so that tokens consumed by the child will not be consumed in the parent
        }
    }
}