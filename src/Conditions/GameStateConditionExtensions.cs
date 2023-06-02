namespace AutoccultistNS
{
    using AutoccultistNS.GameState;

    public static class GameStateConditionExtensions
    {
        public static bool IsConditionMet(this IGameStateCondition condition, IGameState state)
        {
            return condition.IsConditionMet(state, out _);
        }
    }
}
