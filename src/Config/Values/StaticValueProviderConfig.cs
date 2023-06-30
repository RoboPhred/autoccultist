namespace AutoccultistNS.Config.Values
{
    using AutoccultistNS.GameState;

    public class StaticValueProviderConfig : ValueProviderObjectConfig
    {
        public StaticValueProviderConfig()
        {
        }

        public StaticValueProviderConfig(float value)
        {
            this.Value = value;
        }

        public float Value { get; set; }

        public override float GetValue(IGameState state)
        {
            return this.Value;
        }
    }
}
