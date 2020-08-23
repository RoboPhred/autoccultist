namespace Autoccultist.Config
{
    using Autoccultist.Brain;
    using Autoccultist.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// Provides configuration for solving a mansus event.
    /// </summary>
    public class MansusSolutionConfig : IConfigObject, IMansusSolution, IAfterYamlDeserialization
    {
        /// <summary>
        /// Gets or sets the card choice to match against the face up card.
        /// </summary>
        public CardChoiceConfig FaceUpCard { get; set; }

        /// <summary>
        /// Gets or sets the fallback deck to draw from if the face up card does not match <see cref="FaceUpCard"/>.
        /// </summary>
        public string Deck { get; set; }

        /// <inheritdoc/>
        ICardChooser IMansusSolution.FaceUpCard => this.FaceUpCard;

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (this.Deck == null)
            {
                throw new InvalidConfigException("fallbackDeckName is required.");
            }
        }
    }
}
