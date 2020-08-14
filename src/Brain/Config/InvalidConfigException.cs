using System;

namespace Autoccultist.Brain.Config
{
    public class InvalidConfigException : Exception
    {
        public InvalidConfigException(string message) : base(message) { }
    }
}