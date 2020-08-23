namespace Autoccultist.Config
{
    using Autoccultist.Brain;

    /// <summary>
    /// Provides configuration for solving a mansus event.
    /// </summary>
    public class MansusSolutionConfig : IConfigObject, IMansusSolution
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
        public void Validate()
        {
            if (this.Deck == null)
            {
                throw new InvalidConfigException("fallbackDeckName is required.");
            }
        }
    }
}
