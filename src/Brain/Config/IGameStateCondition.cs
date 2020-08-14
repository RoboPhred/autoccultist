namespace Autoccultist.Brain.Config
{
    interface IGameStateCondition
    {
        bool IsConditionMet(IGameState state);
    }
}