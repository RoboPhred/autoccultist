namespace AutoccultistNS.Config.Conditions
{
    using AutoccultistNS.GameState;

    public class FixedCondition : IGameStateCondition
    {
        public static readonly FixedCondition AlwaysPass = new(true);

        public static readonly FixedCondition AlwaysFail = new(false);

        private bool returnSuccess;

        private FixedCondition(bool returnSuccess)
        {
            this.returnSuccess = returnSuccess;
        }

        public ConditionResult IsConditionMet(IGameState state)
        {
            if (!this.returnSuccess)
            {
                return new GeneralConditionFailure("FixedCondition is false.");
            }

            return ConditionResult.Success;
        }
    }
}
