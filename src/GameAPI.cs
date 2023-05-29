namespace AutoccultistNS
{
    using System;
    using System.Linq;
    using SecretHistories.Constants;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Fucine;
    using SecretHistories.Spheres;
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
