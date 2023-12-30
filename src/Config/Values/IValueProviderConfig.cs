namespace AutoccultistNS.Config.Values
{
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;

    [CustomDeserializer(typeof(IValueProviderConfigDeserializer))]
    public interface IValueProviderConfig : IConfigObject
    {
        /// <summary>
        /// Gets the value for a given game state.
        /// </summary>
        float GetValue(IGameState state);
    }
}
