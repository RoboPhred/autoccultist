using Autoccultist.Brain;

namespace Autoccultist.src.Brain.Util
{
    public interface ICondition
    {
        bool IsConditionMet(IGameState state);
    }
}