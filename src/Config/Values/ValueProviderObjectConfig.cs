namespace AutoccultistNS.Config.Values
{
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;

    [DuckTypeCandidate(typeof(StaticValueProviderConfig))]
    public abstract class ValueProviderObjectConfig : NamedConfigObject, IValueProviderConfig
    {
        public abstract float GetValue(IGameState state);
    }
}
