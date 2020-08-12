using System;

namespace Autoccultist.Brain
{

    class OperationFailedException : Exception
    {
        public OperationFailedException(string message) : base(message) { }
    }
}