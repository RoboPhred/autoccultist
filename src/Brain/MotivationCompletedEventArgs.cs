namespace Autoccultist.Brain
{
    using System;

    /// <summary>
    /// An event raised when a motivation is completed.
    /// </summary>
    public class MotivationCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MotivationCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="completedMotivation">The completed goal.</param>
        public MotivationCompletedEventArgs(IMotivation completedMotivation)
        {
            this.CompletedMotivation = completedMotivation;
        }

        /// <summary>
        /// Gets the completed goal.
        /// </summary>
        public IMotivation CompletedMotivation { get; }
    }
}
