using AutoccultistNS.GameState;

namespace AutoccultistNS.Config.Conditions
{
    public class MemoryCondition : ConditionConfig
    {
        public string Memory { get; set; }
        public ValueCondition Value { get; set; }

        public override ConditionResult IsConditionMet(IGameState state)
        {
            // This is called when the game is not running???
            if (!GameAPI.IsRunning)
            {
                try
                {
                    throw new System.Exception("debug");
                }
                catch (System.Exception ex)
                {
                    NoonUtility.LogWarning($"Ignoring attempt to check memory condition while game is not running: {ex.ToString()}");
                    return ConditionResult.Failure;
                }
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