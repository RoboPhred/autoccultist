namespace AutoccultistNS.Brain
{
    public class ImpulseEventArgs : ImperativeEventArgs
    {
        public ImpulseEventArgs(IImperative imperative, IImpulse impulse)
            : base(imperative)
        {
            this.Impulse = impulse;
        }

        public IImpulse Impulse { get; }
    }
}
