namespace AutoccultistNS.Config.Conditions
{
    using System.Linq;
    using AutoccultistNS.GameState;

    public class MemoryCondition : ConditionConfig
    {
        public string Memory { get; set; }

        public ValueCondition Value { get; set; }

        public override ConditionResult IsConditionMet(IGameState state)
        {
            NoonUtility.LogWarning($"Memories in state: {string.Join(", ", state.Memories.Select(x => $"{x.Key}={x.Value}"))}");
            state.Memories.TryGetValue(this.Memory, out var value);
            if (!this.Value.IsConditionMet(value))
            {
                return AddendedConditionResult.Addend(ConditionResult.Failure, $"for memory {this.Memory} value {value} {this.Value}");
            }

            return ConditionResult.Success;
        }
    }
}
