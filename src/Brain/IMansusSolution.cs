namespace Autoccultist.Brain
{
    /// <summary>
    /// A solution to choosing a card from the mansus.
    /// </summary>
    public interface IMansusSolution
    {
        /// <summary>
        /// Gets a card chooser to choose the face up card.
        /// </summary>
        ICardChooser FaceUpCardChooser { get; }

        /// <summary>
        /// Gets the deck name to draw from if the <see cref="FaceUpCardChooser"/> declines to choose a card.
        /// </summary>
        string FallbackDeckName { get; }
    }
}