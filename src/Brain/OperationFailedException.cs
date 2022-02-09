namespace Autoccultist.Brain
{
    using System;

    /// <summary>
    /// Signifies the failure of an operation to execute.
    /// </summary>
    public class OperationFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationFailedException"/> class.
        /// </summary>
        public OperationFailedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationFailedException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public OperationFailedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationFailedException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public OperationFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
