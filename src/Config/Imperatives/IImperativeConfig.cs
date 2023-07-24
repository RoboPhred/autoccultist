namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;

    public interface IImperativeConfig : INamedConfigObject, IImperative
    {
        /// <summary>
        /// Configures the settings for this imperative's UI.
        /// </summary>
        UISettingsConfig UI { get; }
    }
}
