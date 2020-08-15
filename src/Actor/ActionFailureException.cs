namespace Autoccultist.Actor
{
    using System;

    /// <summary>
    /// Indicates a failure to execute an action.
    /// </summary>
    public class ActionFailureException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionFailureException"/> class.
        /// </summary>
        /// <param name="action">The action that caused the error.</param>
        /// <param name="message">The exception message.</param>
        public ActionFailureException(IAutoccultistAction action, string message)
            : base(action.GetType().Name + ": " + message)
        {
            this.Action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionFailureException"/> class.
        /// </summary>
        /// <param name="action">The action that caused the error.</param>
        public ActionFailureException(IAutoccultistAction action)
        {
            this.Action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionFailureException"/> class.
        /// </summary>
        /// <param name="action">The action that caused the error.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ActionFailureException(IAutoccultistAction action, string message, Exception innerException)
            : base(action.GetType().Name + ": " + message, innerException)
        {
            this.Action = action;
        }

        /// <summary>
        /// Gets the action that failed to execute.
        /// </summary>
        public IAutoccultistAction Action { get; }
    }
}
