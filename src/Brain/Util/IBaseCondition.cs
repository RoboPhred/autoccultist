namespace Autoccultist.src.Brain.Util
{
    interface IBaseCondition
    {
        bool IsConditionMet(IGameState state);
    }
}