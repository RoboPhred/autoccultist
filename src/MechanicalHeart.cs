namespace AutoccultistNS
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Update manager for all Autoccultist mechanisms.
    /// <para>
    /// Unlike the Cultist Simulator heart, this beats while the game is paused.
    /// </summary>
    public static class MechanicalHeart
    {
        static MechanicalHeart()
        {
            GameEventSource.GameEnded += OnGameEnded;
        }

        /// <summary>
        /// Raised when the mechanical heart beats.
        /// </summary>
        public static event EventHandler<EventArgs> OnBeat;

        public static event EventHandler<EventArgs> OnStart;

        public static event EventHandler<EventArgs> OnStop;

        /// <summary>
        /// Gets a value indicating whether the mechanical heart is running in time with the standard heart.
        /// </summary>
        public static bool IsRunning { get; private set; } = false;

        /// <summary>
        /// Sets the mechanical heart beating in time with the standard heart.
        /// </summary>
        public static void Start()
        {
            // Don't run if the game isn't running.
            if (!GameAPI.IsRunning)
            {
                Autoccultist.LogWarn("Ignoring Mechanical Heart start: game not running.");
                return;
            }

            if (IsRunning)
            {
                Autoccultist.LogWarn("Ignoring Mechanical Heart start: already running.");
                return;
            }

            Autoccultist.LogTrace("Starting Mechanical Heart.");

            IsRunning = true;
            OnStart?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Stops the mechanical heart beating.
        /// </summary>
        public static void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            Autoccultist.LogTrace("Stopping Mechanical Heart.");

            IsRunning = false;
            OnStop?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Steps one beat.
        /// </summary>
        public static void Step()
        {
            Stop();
            TriggerMechanicalBeat();
        }

        public static async Task AwaitStart(CancellationToken cancellationToken)
        {
            await EventHandlerExtensions.AwaitEvent<EventArgs>(h => OnStart += h, h => OnStart -= h, cancellationToken);
        }

        public static async Task AwaitBeatIfStopped(CancellationToken cancellationToken)
        {
            if (!IsRunning)
            {
                await AwaitBeat(cancellationToken);
            }
        }

        public static async Task AwaitBeat(CancellationToken cancellationToken)
        {
            await EventHandlerExtensions.AwaitEvent<EventArgs>(h => OnBeat += h, h => OnBeat -= h, cancellationToken);
        }

        public static async Task AwaitBeat(CancellationToken cancellationToken, TimeSpan delay)
        {
            if (delay == TimeSpan.Zero)
            {
                throw new ArgumentException("Delay must be greater than zero.", nameof(delay));
            }

            // Wait out the delay, then wait for the next beat
            await RealtimeDelay.Of(delay, cancellationToken);
            await AwaitBeat(cancellationToken);
        }

        /// <summary>
        /// Updates the Mechanical Heart.
        /// </summary>
        public static void Update()
        {
            if (IsRunning)
            {
                TriggerMechanicalBeat();
            }
        }

        private static void OnGameEnded(object sender, EventArgs e)
        {
            IsRunning = false;
        }

        private static void TriggerMechanicalBeat()
        {
            if (!GameAPI.IsInteractable)
            {
                return;
            }

            try
            {
                OnBeat?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Autoccultist.LogWarn($"Error in Mechanical Heart: {ex.Message}");
                NoonUtility.LogException(ex);
                MechanicalHeart.Stop();
            }
        }
    }
}
