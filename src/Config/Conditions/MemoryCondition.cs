namespace AutoccultistNS.Config.Conditions
{
    using AutoccultistNS.GameState;

    public class MemoryCondition : ConditionConfig
    {
        public string Memory { get; set; }

        public ValueCondition Value { get; set; }

        public override ConditionResult IsConditionMet(IGameState state)
        {
            if (!this.Value.IsConditionMet(Hippocampus.GetMemory(this.Memory, state)))
            {
                return AddendedConditionResult.Addend(ConditionResult.Failure, $"for memory {this.Memory} against {this.Value}");
            }

            return ConditionResult.Success;
        }
    }
}
