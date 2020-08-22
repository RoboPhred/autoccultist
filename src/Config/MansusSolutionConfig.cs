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
        public CardChoiceConfig FaceUpCardChooser { get; set; }

        /// <summary>
        /// Gets or sets the fallback deck to draw from if the face up card does not match <see cref="FaceUpCardChooser"/>.
        /// </summary>
        public string FallbackDeckName { get; set; }

        /// <inheritdoc/>
        ICardChooser IMansusSolution.FaceUpCardChooser => this.FaceUpCardChooser;

        /// <inheritdoc/>
        public void Validate()
        {
            if (this.FallbackDeckName == null)
            {
                throw new InvalidConfigException("fallbackDeckName is required.");
            }
        }
    }
}
