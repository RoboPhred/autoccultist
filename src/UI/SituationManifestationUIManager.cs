namespace AutoccultistNS.UI
{
    using System;
    using System.Linq;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Events;
    using SecretHistories.Fucine;

    public class SituationManifestationUIManager : ISphereEventSubscriber
    {
        private static SituationManifestationUIManager instance;

        private SituationManifestationUIManager()
        {
        }

        public static void Initialize()
        {
            if (instance == null)
            {
                instance = new SituationManifestationUIManager();
                GameEventSource.GameStarted += HandleGameStarted;
                if (GameAPI.IsRunning)
                {
                    HandleGameStarted(null, EventArgs.Empty);
                }
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
                    SituationAdded(situation);
                    break;
            }
        }

        private static void HandleGameStarted(object sender, EventArgs e)
        {
            GameAPI.TabletopSphere.Subscribe(instance);
            foreach (var situation in GameAPI.TabletopSphere.GetTokens().Select(t => t.Payload).OfType<Situation>())
            {
                SituationAdded(situation);
            }
        }

        private static void SituationAdded(Situation situation)
        {
            SituationAutomationButton.AttachToSituation(situation);
        }
    }
}
