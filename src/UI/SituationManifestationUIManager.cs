namespace AutoccultistNS.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Events;
    using SecretHistories.Fucine;

    public class SituationManifestationUIManager : ISphereEventSubscriber
    {
        private static readonly Dictionary<Situation, SituationManifestationUI> UIBySituation = new();
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
                GameEventSource.GameEnded += HandleGameEnded;
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

        private static void HandleGameEnded(object sender, EventArgs e)
        {
            foreach (var ui in UIBySituation.Values)
            {
                ui.Dispose();
            }

            UIBySituation.Clear();
        }

        private static void SituationAdded(Situation situation)
        {
            if (UIBySituation.ContainsKey(situation))
            {
                return;
            }

            situation.OnChanged += SituationChanged;

            var ui = new SituationManifestationUI(situation);
            UIBySituation.Add(situation, ui);
        }

        private static void SituationChanged(TokenPayloadChangedArgs args)
        {
            if (args.Payload is not Situation situation)
            {
                return;
            }

            if (args.ChangeType == PayloadChangeType.Retirement)
            {
                SituationRemoved(situation);
            }
        }

        private static void SituationRemoved(Situation situation)
        {
            if (!UIBySituation.TryGetValue(situation, out var ui))
            {
                return;
            }

            ui.Dispose();
            UIBySituation.Remove(situation);
        }
    }
}
