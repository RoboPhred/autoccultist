using System;

namespace Autoccultist.Brain.Config
{
    class InvalidConfigException : Exception
    {
        public InvalidConfigException(string message) : base(message) { }
    }
}