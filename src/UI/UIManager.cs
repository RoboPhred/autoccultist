namespace AutoccultistNS.UI
{
    using System;
    using System.Linq;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Events;
    using SecretHistories.Fucine;
    using UnityEngine;

    public class UIManager : ISphereEventSubscriber
    {
        private static UIManager instance;

        private IconButtonWidget automationsButton;

        private AutomationsWindow automationsWindow;

        private UIManager()
        {
            GameEventSource.GameStarted += this.HandleGameStarted;
            if (GameAPI.IsRunning)
            {
                this.HandleGameStarted(null, EventArgs.Empty);
            }
        }

        public static void Initialize()
        {
            if (instance == null)
            {
                instance = new UIManager();
            }
        }

        void ISphereEventSubscriber.OnSphereChanged(SphereChangedArgs args)
        {
        }

        void ISphereEventSubscriber.OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
        }

        void ISphereEventSubscriber.OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            var situation = args.Token.Payload as Situation;
            if (situation == null)
            {
                return;
            }

            switch (args.Change)
            {
                case SphereContentChange.TokenAdded:
                    this.SituationAdded(situation);
                    break;
            }
        }

        private void HandleGameStarted(object sender, EventArgs e)
        {
            GameAPI.TabletopSphere.Subscribe(instance);
            foreach (var situation in GameAPI.TabletopSphere.GetTokens().Select(t => t.Payload).OfType<Situation>())
            {
                this.SituationAdded(situation);
            }

            this.automationsWindow = AutomationsWindow.CreateMetaWindow<AutomationsWindow>("AutomationsWindow");

            this.automationsButton = MountPoints.MetaWindowLayer.AddIconButton("AutomationsWindowButton")
                .Left(0, 5)
                .Top(1, -5)
                .Right(0, 50)
                .Bottom(1, -50)
                .Background()
                .Sprite("gears_icon")
                .OnClick(() =>
                {
                    if (this.automationsWindow.IsOpen)
                    {
                        this.automationsWindow.Close();
                    }
                    else
                    {
                        // FIXME: meta-window OpenAt position not working.
                        this.automationsWindow.OpenAt(new Vector2(-600, 90));
                    }
                });
        }

        private void SituationAdded(Situation situation)
        {
            SituationAutomationButton.AttachToSituation(situation);
        }
    }
}
