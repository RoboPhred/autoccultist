using System.Collections.Generic;
using Autoccultist.Brain.Config;

namespace Autoccultist.Brain.Util
{
    interface ITask
    {
        string Name { get; }

        bool ShouldExecute(IGameState state);
        bool IsSatisfied(IGameState state);
        IList<Imperative> GetImperatives();
    }
}
