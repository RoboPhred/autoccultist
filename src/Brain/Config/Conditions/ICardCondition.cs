using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autoccultist.Brain.Config.Conditions
{
    public interface ICardCondition : IGameStateConditionConfig
    {
        List<CardChoice> CardSet { get; }
    }
}
