using System.Collections.Generic;

namespace Autoccultist.GameState
{
    // A card present on a table, which is consumable
    public class TableCardState : IGameItemState, IHasAspects, IConsumableStateToken
    {
        public int LifetimeRemaining => throw new System.NotImplementedException();

        public string TokenId => throw new System.NotImplementedException();

        public IReadOnlyDictionary<string, int> Aspects => throw new System.NotImplementedException();

        public IConsumedToken Consume()
        {
            throw new System.NotImplementedException();
        }
    }
}