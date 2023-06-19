namespace AutoccultistNS.Brain
{
    using System;

    public class ReactionFailedException : Exception
    {
        public ReactionFailedException(string message) : base(message) { }

        public ReactionFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
