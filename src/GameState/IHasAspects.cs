using System.Collections.Generic;

namespace Autoccultist.GameState
{
    public interface IHasAspects
    {
        IReadOnlyDictionary<string, int> Aspects { get; }
    }
}