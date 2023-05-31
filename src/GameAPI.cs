namespace AutoccultistNS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Assets.Scripts.Application.UI;
    using SecretHistories.Constants;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Fucine;
    using SecretHistories.Spheres;
    using SecretHistories.Tokens.Payloads;
    using SecretHistories.UI;
    using UnityEngine;

    /// <summary>
    /// A set of static functions for manipulating the game.
    /// </summary>
    public static class GameAPI
    {
        private static GameSpeed prePauseSpeed = GameSpeed.Normal;
        private static int pauseDepth = 0;

        /// <summary>
        /// Gets a value indicating whether the game is running.
        /// </summary>
        public static bool IsRunning { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the game is interactable.
        /// </summary>
        public static bool IsInteractable
        {
            get
            {
                if (!IsRunning)
                {
                    return false;
                }

                // Mansus sets delays using this, so this might be a good check instead of IsMansusInteractable
                if (Watchman.Get<LocalNexus>().PlayerInputDisabled())
                {
                    return false;
                }

                // Apparently the above isn't good enough to wait on the mansus, so let's wait for the cards to appear.
                if (IsInMansus && GetMansusChoices(out var _) == null)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the mansus is active.
        /// </summary>
        public static bool IsInMansus
        {
            get
            {
                return Watchman.Get<Numa>().IsOtherworldActive();
            }
        }

        /// <summary>
        /// Gets the current user-chosen game speed.
        /// </summary>
        public static GameSpeed GameSpeed
        {
            get
            {
                var heartGo = GameObject.Find("Heart");
                if (heartGo == null)
                {
                    NoonUtility.LogWarning("Could not find Heart.");
                    return GameSpeed.Paused;
                }

                var heartBehavior = heartGo.GetComponent<Heart>();

                var speedState = Reflection.GetPrivateField<GameSpeedState>(heartBehavior, "gameSpeedState");
                return speedState.GetEffectiveGameSpeed();
            }
        }

        /// <summary>
        /// Initialize the GameAPI.
        /// <para>
        /// Cannot be a static constructor, as this must run early, before GameAPI is naturally used.
        /// </summary>
        public static void Initialize()
        {
            GameEventSource.GameStarted += OnGameStarted;
            GameEventSource.GameEnded += OnGameEnded;
        }

        /// <summary>
        /// Gets the situation with the given id.
        /// </summary>
        /// <param name="situationId">The id of the situation to get.</param>
        /// <returns>The situation with the given id, or null if no such situation exists.</returns>
        public static Situation GetSituation(string situationId)
        {
            var situations = Watchman.Get<HornedAxe>().GetRegisteredSituations();
            return situations.FirstOrDefault(x => x.VerbId == situationId);
        }

        /// <summary>
        /// Try to slots the card into the given sphere, if accepted by its spec.
        /// </summary>
        /// <param name="cardId">The id of the card to slot.</param>
        /// <returns>True if successful, or false if the card was not accepted..</returns>
        public static bool TrySlotCard(Sphere sphere, ElementStack card)
        {
            var token = card.Token;
            if (!sphere.CanAcceptToken(token))
            {
                if (!sphere.HasEnoughSpaceForToken(token))
                {
                    Autoccultist.Instance.LogWarn($"Rejecting sloting of {card.EntityId} into sphere {sphere.Id} because there is not enough space.");
                }
                else if (!sphere.IsValidDestinationForToken(token))
                {
                    Autoccultist.Instance.LogWarn($"Rejecting sloting of {card.EntityId} into sphere {sphere.Id} because it is not a valid destination.");
                }
                else
                {
                    Autoccultist.Instance.LogWarn($"Rejecting sloting of {card.EntityId} into sphere {sphere.Id} for unknown reason.");
                }

                return false;
            }

            sphere.AcceptToken(token, new Context(Context.ActionSource.DoubleClickSend));

            return true;

            // This is what double click does.
            // Might want to use this instead, if we can accept the time delay as the card transfers.
            // Currently, we are set up to assume the card is slotted immediately.  We would need to make the
            // SlotCardAction await the slotting for this to be of use.

            // this.sphere.GetItineraryFor(elementStack.Token).WithDuration(0.3f).Depart(elementStack, new Context(Context.ActionSource.DoubleClickSend));
        }

        public static IReadOnlyDictionary<string, ElementStack> GetMansusChoices(out string faceUpDeckName)
        {
            faceUpDeckName = null;

            try
            {
                var spheres = GetMansusSpheres(out faceUpDeckName, out var _, out var _);
                var value = new Dictionary<string, ElementStack>();
                foreach (var sphere in spheres)
                {
                    var token = sphere.Value.GetTokens().FirstOrDefault();
                    if (!token)
                    {
                        // Mansus isn't ready.
                        // This could be because we have not drawn yet, or because the user has made a choice and we are now idle.
                        return null;
                    }

                    value.Add(sphere.Key, token.Payload as ElementStack);
                }

                return spheres.ToDictionary(x => x.Key, x => x.Value.GetTokens().First().Payload as ElementStack);
            }
            catch (Exception ex)
            {
                NoonUtility.LogWarning($"Exception in GameAPI.GetMansusChoices: {ex.ToString()}");
                return null;
            }
        }

        public static bool ChooseMansusDeck(string deckName)
        {
            try
            {
                var spheres = GetMansusSpheres(out var _, out var otherworld, out var ingress);
                if (!spheres.TryGetValue(deckName, out var sphere))
                {
                    return false;
                }

                var dominions = Reflection.GetPrivateField<List<OtherworldDominion>>(otherworld, "_dominions");

                var dominion = dominions.FirstOrDefault(x => x.MatchesEgress(ingress.GetEgressId()));
                if (dominion == null)
                {
                    throw new Exception($"Could not find dominion for ingress {ingress.Id} in otherworld {otherworld.Id}");
                }

                var token = sphere.GetTokens().First();

                if (!dominion.EgressSphere.TryAcceptToken(token, new Context(Context.ActionSource.PlayerDrag)))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                NoonUtility.LogWarning($"Exception in GameAPI.ChooseMansusDeck: {ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Sets the pause state of the game.
        /// </summary>
        /// <returns>A token to unpause the game.</returns>
        public static PauseToken Pause()
        {
            if (pauseDepth == 0)
            {
                prePauseSpeed = GameSpeed;
                if (IsRunning && prePauseSpeed != GameSpeed.Paused)
                {
                    Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs()
                    {
                        ControlPriorityLevel = 1,
                        GameSpeed = GameSpeed.Paused,
                        WithSFX = true,
                    });
                }
            }

            pauseDepth++;

            return new PauseToken();
        }

        /// <summary>
        /// Display a notification toast to the user.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message of the toast.</param>
        public static void Notify(string title, string message)
        {
            var notifier = Watchman.Get<Notifier>();
            if (notifier == null)
            {
                return;
            }

            notifier.ShowNotificationWindow(title, message, false);
        }

        private static IReadOnlyDictionary<string, Sphere> GetMansusSpheres(out string faceUpDeckName, out Otherworld otherworld, out Ingress ingress)
        {
            ingress = null;
            otherworld = null;
            faceUpDeckName = null;

            var numa = Watchman.Get<Numa>();
            var compendium = Watchman.Get<Compendium>();

            otherworld = Reflection.GetPrivateField<Otherworld>(numa, "_currentOtherworld");
            if (otherworld == null)
            {
                return null;
            }

            ingress = Reflection.GetPrivateField<Ingress>(otherworld, "_activeIngress");
            if (ingress == null)
            {
                return null;
            }

            var spheres = otherworld.GetSpheres();

            var consequences = ingress.GetConsequences();

            var faceUpDeck = compendium.GetEntityById<Recipe>(consequences[0].Id);
            var deckOne = compendium.GetEntityById<Recipe>(consequences[1].Id);
            var deckTwo = compendium.GetEntityById<Recipe>(consequences[2].Id);

            if (faceUpDeck == null || faceUpDeck.DeckEffects.Keys.Count == 0)
            {
                throw new Exception("FaceUpDeck is null or has no DeckEffects");
            }

            if (deckOne == null || deckOne.DeckEffects.Keys.Count == 0)
            {
                throw new Exception("DeckOne is null or has no DeckEffects");
            }

            if (deckTwo == null || deckTwo.DeckEffects.Keys.Count == 0)
            {
                throw new Exception("DeckTwo is null or has no DeckEffects");
            }

            faceUpDeckName = faceUpDeck.DeckEffects.Keys.First();
            var deckOneName = deckOne.DeckEffects.Keys.First();
            var deckTwoName = deckTwo.DeckEffects.Keys.First();

            // FIXME: This is nasty, but HornexAxe.GetSphereByPath(FromPath, overworld) doesn't find these.
            // Which is strange, as the error message it gives contains the exact same path as we see from sphere.GetAbsolutePath().Path
            var faceUpSphere = spheres.FirstOrDefault(x => "/" + x.Id == consequences[0].ToPath.Path);
            var deckOneSphere = spheres.FirstOrDefault(x => "/" + x.Id == consequences[1].ToPath.Path);
            var deckTwoSphere = spheres.FirstOrDefault(x => "/" + x.Id == consequences[2].ToPath.Path);

            return new Dictionary<string, Sphere>
            {
                { faceUpDeckName, faceUpSphere },
                { deckOneName, deckOneSphere },
                { deckTwoName, deckTwoSphere },
            };
        }

        private static void OnGameStarted(object sender, EventArgs e)
        {
            IsRunning = true;
        }

        private static void OnGameEnded(object sender, EventArgs e)
        {
            IsRunning = false;
        }

        /// <summary>
        /// A token representing a pause.
        /// </summary>
        public class PauseToken : IDisposable
        {
            private bool isDisposed = false;

            /// <summary>
            /// Finalizes an instance of the <see cref="PauseToken"/> class.
            /// </summary>
            ~PauseToken()
            {
                NoonUtility.LogWarning("Leaked PauseToken");
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                if (this.isDisposed)
                {
                    return;
                }

                this.isDisposed = true;
                GC.SuppressFinalize(this);
                pauseDepth--;
                if (pauseDepth == 0 && prePauseSpeed != GameSpeed.Paused)
                {
                    Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(new SpeedControlEventArgs()
                    {
                        ControlPriorityLevel = 1,
                        GameSpeed = prePauseSpeed,
                        WithSFX = true,
                    });
                }
            }
        }
    }
}
