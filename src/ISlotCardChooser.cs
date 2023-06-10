namespace AutoccultistNS
{
    public interface ISlotCardChooser : ICardChooser
    {
        /// <summary>
        /// Gets a value indicating whether this slot is optional.
        /// </summary>
        bool Optional { get; }
    }
}