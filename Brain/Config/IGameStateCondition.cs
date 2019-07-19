
namespace Autoccultist.Brain.Config
{
    public interface IGameStateCondition
    {
        bool IsConditionMet(IGameState state);
    }
}