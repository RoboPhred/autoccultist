namespace AutoccultistNS.GameState
{
    public interface ISituationSlot
    {
        /// <summary>
        /// Gets the id of the specification of this slot.
        /// </summary>
        string SpecId { get; }

        /// <summary>
        /// Gets the card in the slot, if any.
        /// </summary>
        ICardState Card { get; }
    }
}
