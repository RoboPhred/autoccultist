namespace AutoccultistNS
{
    using System;

    [Flags]
    public enum CardChooserHints
    {
        None = 0,

        /// <summary>
        /// Indicates that the consumer does not care about the priority of the choices.
        /// This is often used when the consumer wants every single match and does not do anything
        /// dependent on the order.
        /// </summary>
        IgnorePriority = 1,
    }
}
