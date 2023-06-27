namespace AutoccultistNS.Config.Conditions
{
    using AutoccultistNS.GameState;

    public class MemoryCondition : ConditionConfig
    {
        public string Memory { get; set; }

        public ValueCondition Value { get; set; }

        public override ConditionResult IsConditionMet(IGameState state)
        {
            if (!GameAPI.IsRunning)
            {
                // FIXME: This is needed because we do a dry run on conditions.
                // - Do not do a dry run, instead check for cyclic motivations properly.
                // - Get memory from IGameState
                return ConditionResult.Failure;
            }

            // TODO: Bit awkward calling out to Hippocampus directly.  Should probably store memory values in IGameState
            // You know, for that unit testing i'm never going to do.
            if (!this.Value.IsConditionMet(Hippocampus.GetMemory(this.Memory)))
            {
                return AddendedConditionResult.Addend(ConditionResult.Failure, $"for memory {this.Memory} against {this.Value}");
            }

            return ConditionResult.Success;
        }
    }
}
