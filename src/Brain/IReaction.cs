namespace AutoccultistNS.Brain
{
    using System;

    public interface IReaction
    {
        /// <summary>
        /// Raised when this reaciton execution completes.
        /// </summary>
        event EventHandler<ReactionEndedEventArgs> Ended;

        /// <summary>
        /// Abort the execution.
        /// </summary>
        void Abort();

        /// <summary>
        /// Start the reaction.
        /// </summary>
        void Start();
    }
}
