namespace Autoccultist
{
    using System;
    using Autoccultist.Actor;
    using Autoccultist.Brain;

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
                AutoccultistPlugin.Instance.LogTrace("Ignoring Mechanical Heart start: game not running.");
                return;
            }

            if (IsRunning)
            {
                AutoccultistPlugin.Instance.LogTrace("Ignoring Mechanical Heart start: already running.");
                return;
            }

            AutoccultistPlugin.Instance.LogTrace("Starting Mechanical Heart.");

            IsRunning = true;
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

            AutoccultistPlugin.Instance.LogTrace("Stopping Mechanical Heart.");

            IsRunning = false;
        }

        /// <summary>
        /// Steps one beat.
        /// </summary>
        public static void Step()
        {
            AutoccultistPlugin.Instance.LogTrace("Stepping Mechanical Heart.");
            Stop();
            TriggerMechanicalBeat();
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
            if (!GameAPI.IsRunning)
            {
                return;
            }

            GoalDriver.Update();
            AutoccultistActor.Update();
            SituationOrchestrator.Update();
        }
    }
}
