using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autoccultist.Brain.Util
{
    interface ITask
    {
        string Name { get; }

        bool ShouldExecute(IGameState state);
        bool IsSatisfied(IGameState state);
    }
}
