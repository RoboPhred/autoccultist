namespace Autoccultist.Brain
{
    using System;

    /// <summary>
    /// An event raised when a goal is completed.
    /// </summary>
    public class GoalCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoalCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="completedGoal">The completed goal.</param>
        public GoalCompletedEventArgs(IGoal completedGoal)
        {
            this.CompletedGoal = completedGoal;
        }

        /// <summary>
        /// Gets the completed goal.
        /// </summary>
        public IGoal CompletedGoal { get; }
    }
}
