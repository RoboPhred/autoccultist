namespace AutoccultistNS.Config.Conditions
{
    using AutoccultistNS.GameState;

    public class MemoryCondition : ConditionConfig
    {
        public string Memory { get; set; }

        public ValueCondition Value { get; set; }

        public override ConditionResult IsConditionMet(IGameState state)
        {
            var value = Hippocampus.GetMemory(this.Memory, state);
            if (!this.Value.IsConditionMet(value))
            {
                return AddendedConditionResult.Addend(ConditionResult.Failure, $"for memory {this.Memory} value {value} {this.Value}");
            }

            return ConditionResult.Success;
        }
    }
}
