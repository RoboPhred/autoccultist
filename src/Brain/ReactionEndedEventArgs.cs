namespace AutoccultistNS.Brain
{
    using System;

    public class ReactionEndedEventArgs : EventArgs
    {
        public ReactionEndedEventArgs(bool aborted)
        {
            this.Aborted = aborted;
        }

        public bool Aborted { get; }
    }
}
