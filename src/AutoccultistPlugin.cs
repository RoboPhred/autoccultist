namespace Autoccultist
{
    using System;
    using System.IO;
    using Autoccultist.Actor;
    using Autoccultist.Brain;
    using Autoccultist.Brain.Config;
    using Autoccultist.GameState;
    using Autoccultist.GUI;
    using UnityEngine;

    /// <summary>
    /// The main entrypoint for Autoccultist, loaded by BenInEx.
    /// </summary>
    [BepInEx.BepInPlugin("net.robophreddev.CultistSimulator.Autoccultist", "Autoccultist", "0.0.1")]
    public class AutoccultistPlugin : BepInEx.BaseUnityPlugin
    {
        private bool isRunning = false;

        private AutoccultistBrain brain;

        /// <summary>
        /// Gets the instance of the plugin.
        /// </summary>
        public static AutoccultistPlugin Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the directory the mod dll is located in.
        /// </summary>
        public static string AssemblyDirectory
        {
            get
            {
                var assemblyLocation = typeof(AutoccultistPlugin).Assembly.Location;
                var assemblyDir = Path.GetDirectoryName(assemblyLocation);
                return assemblyDir;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the brain is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }

        public AutoccultistBrain Brain
        {
            get
            {
                return this.brain;
            }
        }

        /// <summary>
        /// Starts the mod.
        /// </summary>
        public void Start()
        {
            Instance = this;

            var brainConfig = this.LoadBrainConfig();
            this.LogInfo($"Loaded {brainConfig.Goals.Count} goals.");

            this.brain = new AutoccultistBrain(brainConfig.Goals);

            this.LogInfo("Autoccultist initialized.");
        }

        public void ResetBrain()
        {
            AutoccultistPlugin.Instance.LogInfo("Resetting brain");
            var config = this.LoadBrainConfig();
            this.brain.Reset(config.Goals);
        }

        public void StartBrain()
        {
            this.isRunning = true;
        }

        public void StopBrain()
        {
            this.isRunning = false;
        }

        /// <summary>
        /// Renders the mod GUI.
        /// </summary>
        public void OnGUI()
        {
            TestGUI.OnGUI();
        }

        /// <summary>
        /// Runs an update tick on the mod.
        /// </summary>
        public void Update()
        {
            GameStateProvider.Invalidate();

            this.ProcessHotkeys();

            if (this.brain.IsRunning != this.isRunning)
            {
                if (this.isRunning)
                {
                    this.LogInfo("Starting brain");
                    this.brain.Start();
                }
                else
                {
                    this.LogInfo("Stopping brain");
                    this.brain.Stop();
                    SituationOrchestrator.Abort();
                    AutoccultistActor.AbortAllActions();
                }
            }

            if (this.isRunning)
            {
                // The idea was to always update children,
                //  but some things crash if updating when the main game isn't in play.
                // This needs more work.
                this.UpdateChildren();
            }
        }

        /// <summary>
        /// Log an info-level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogInfo(string message)
        {
            this.Logger.LogInfo(message);
        }

        /// <summary>
        /// Log a trace-level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogTrace(string message)
        {
            this.Logger.LogInfo(message);
        }

        /// <summary>
        /// Log a warning-level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogWarn(string message)
        {
            this.Logger.LogWarning(message);
            GameAPI.Notify("Autoccultist Warning", message);
        }

        /// <summary>
        /// Log and handle a fatal event.
        /// This will also stop the brain from running.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Fatal(string message)
        {
            this.Logger.LogError("Fatal - " + message);
            GameAPI.Notify("Autoccultist Fatal", message);
            this.isRunning = false;
            this.brain.Stop();
        }

        private void ProcessHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                if (this.isRunning)
                {
                    this.StopBrain();
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        this.ResetBrain();
                    }
                    else
                    {
                        this.StartBrain();
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                this.LogInfo("Dumping status");
                this.brain.LogStatus(GameStateProvider.Current);
                SituationOrchestrator.LogStatus();
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                this.LogInfo("Dumping situations");
                SituationLogger.LogSituations();
            }
        }

        private BrainConfig LoadBrainConfig()
        {
            var configPath = Path.Combine(AssemblyDirectory, "brain.yml");
            this.LogInfo(string.Format("Loading config from {0}", configPath));
            return BrainConfig.Load(configPath);
        }

        private void UpdateChildren()
        {
            this.brain.Update(GameStateProvider.Current);
            AutoccultistActor.Update();
            SituationOrchestrator.Update();
        }
    }
}
