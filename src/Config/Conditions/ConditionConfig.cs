namespace AutoccultistNS.Config.Conditions
{
    using AutoccultistNS.GameState;

    public abstract class ConditionConfig : NamedConfigObject, IGameStateConditionConfig
    {
        ConditionResult IGameStateCondition.IsConditionMet(IGameState state)
        {
            return this.IsConditionMet(state);
        }

        public abstract ConditionResult IsConditionMet(IGameState state);
    }
}
