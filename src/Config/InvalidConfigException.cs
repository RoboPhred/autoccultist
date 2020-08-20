namespace Autoccultist.Config
{
    using System;

    /// <summary>
    /// Thrown when configuration data is improperly formed.
    /// </summary>
    public class InvalidConfigException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigException"/> class.
        /// </summary>
        public InvalidConfigException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigException"/> class.
        /// </summary>
        /// <param name="message">The message to include with the exception.</param>
        public InvalidConfigException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigException"/> class.
        /// </summary>
        /// <param name="message">The message to include with the exception.</param>
        /// <param name="innerException">The inner exception to pass this the exception.</param>
        public InvalidConfigException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
