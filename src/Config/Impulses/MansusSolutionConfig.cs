namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// Provides configuration for solving a mansus event.
    /// </summary>
    public class MansusSolutionConfig : ConfigObject, IMansusSolution
    {
        /// <summary>
        /// Gets or sets the card choice to match against the face up card.
        /// </summary>
        public CardChooserConfig FaceUpCard { get; set; }

        /// <summary>
        /// Gets or sets the fallback deck to draw from if the face up card does not match <see cref="FaceUpCard"/>.
        /// </summary>
        public string Deck { get; set; }

        /// <inheritdoc/>
        ICardChooser IMansusSolution.FaceUpCard => this.FaceUpCard;

        /// <inheritdoc/>
        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (this.Deck == null)
            {
                throw new InvalidConfigException("deck is required.");
            }
        }
    }
}
