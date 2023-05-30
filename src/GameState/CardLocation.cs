namespace AutoccultistNS.GameState
{
    /// <summary>
    /// Defines locations a card may be in.
    /// </summary>
    public enum CardLocation
    {
        /// <summary>
        /// The card is in the tabletop.
        /// </summary>
        Tabletop,

        /// <summary>
        /// The card is going somewhere.
        /// </summary>
        EnRoute,

        /// <summary>
        /// The card is in a situation slot.
        /// </summary>
        Slotted,

        /// <summary>
        /// The card is stored inside a situation.
        /// </summary>
        Stored,

        /// <summary>
        /// The card is in a mansus deck.
        /// </summary>
        Mansus,
    }
}
