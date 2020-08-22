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
        ICardChooser MansusCardChoice { get; }

        /// <summary>
        /// Gets the deck name to draw from if the <see cref="MansusCardChoice"/> declines to choose a card.
        /// </summary>
        string MansusDefaultDeck { get; }
    }
}
