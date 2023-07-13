namespace AutoccultistNS.UI
{
    using System;
    using SecretHistories.Abstract;
    using SecretHistories.Entities;
    using SecretHistories.Events;
    using SecretHistories.UI;

    public abstract class SituationWindow : AbstractWindow, IPayloadWindow
    {
        public Situation Situation { get; private set; }

        void IPayloadWindow.NotifySpheresChanged(Context context)
        {
        }

        void IPayloadWindow.ContentsDisplayChanged(ContentsDisplayChangedArgs args)
        {
        }

        void IPayloadWindow.Attach(ElementStack elementStack)
        {
            throw new NotSupportedException();
        }

        public void Attach(Situation situation)
        {
            this.Close(true);
            this.Situation = situation;
            this.OnAttach();
        }
    }
}
