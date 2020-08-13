using System.Collections.Generic;

namespace Autoccultist.GameState
{
    public interface IGameState
    {
        IReadOnlyList<IGameItemState> TabledItems { get; }
    }
}