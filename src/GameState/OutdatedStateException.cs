namespace Autoccultist.GameState
{
    using System;

    /// <summary>
    /// An exception signifying that the accessed state object was out of date and cannot be queried.
    /// </summary>
    public class OutdatedStateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutdatedStateException"/> class.
        /// </summary>
        public OutdatedStateException()
            : base("The requested game state object is out of date.  Only state objects corresponding to the current frame can be used.")
        {
        }
    }
}
