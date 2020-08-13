using System;
using System.Runtime.Serialization;

namespace Autoccultist.Brain.Config
{
    [Serializable]
    internal class ConditionNotMetException : Exception
    {
        public ConditionNotMetException(string message) : base(message) { }
}
}