using System;

namespace Autoccultist.Actor
{

    class ActionFailureException : Exception
    {
        public IAutoccultistAction Action { get; private set; }
        public ActionFailureException(IAutoccultistAction action, string message) : base(action.GetType().Name + ": " + message)
        {
            this.Action = action;
        }
    }
}