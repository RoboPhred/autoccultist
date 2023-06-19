namespace AutoccultistNS.Brain
{
    using System;

    /// <summary>
    /// An event raised when a goal is completed.
    /// </summary>
    public class ImperativeEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImperativeEventArgs"/> class.
        /// </summary>
        /// <param name="imperative">The imperative this event is about.</param>
        public ImperativeEventArgs(IImperative imperative)
        {
            this.Imperative = imperative;
        }

        /// <summary>
        /// Gets the imperative.
        /// </summary>
        public IImperative Imperative { get; }
    }
}
