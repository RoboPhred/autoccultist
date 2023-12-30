namespace AutoccultistNS.Brain
{
    public class ReactionEventArgs : ImperativeEventArgs
    {
        public ReactionEventArgs(IImperative imperative, IImpulse impulse, IReaction reaction)
            : base(imperative)
        {
            this.Impulse = impulse;
            this.Reaction = reaction;
        }

        public IImpulse Impulse { get; }

        public IReaction Reaction { get; }
    }
}
