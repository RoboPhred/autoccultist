using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autoccultist.Brain;

namespace Autoccultist.src.Brain.Util
{
    public interface ICondition
    {
        bool IsConditionMet(IGameState state);
    }
}
