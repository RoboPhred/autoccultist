namespace Autoccultist
{
    using System.IO;
    using Autoccultist.Actor;
    using Autoccultist.Brain;
    using Autoccultist.Brain.Config;
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
        /// Starts the mod.
        /// </summary>
        public void Start()
        {
            Instance = this;

            Dispatcher.Initialize();

            var brainConfig = this.LoadBrainConfig();
            this.LogInfo($"Loaded {brainConfig.Goals.Count} goals.");

            this.brain = new AutoccultistBrain(brainConfig);

            this.LogInfo("Autoccultist initialized.");
        }

        /// <summary>
        /// Runs an update tick on the mod.
        /// </summary>
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                if (this.isRunning)
                {
                    this.LogInfo("Stopping brain");
                    this.brain.Stop();
                    this.isRunning = false;
                    if (Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        this.LogInfo("Reloading brain");
                        this.LoadBrainConfig();
                        this.LogInfo("Restarting brain");
                        this.brain.Reset();
                    }
                }
                else
                {
                    this.LogInfo("Starting brain");
                    this.brain.Start();
                    this.isRunning = true;
                }
            }
            else if (Input.GetKeyDown(KeyCode.F10))
            {
                // Ensure not running
                this.isRunning = false;
                this.LogInfo("Step");
                this.brain.Start();
                this.UpdateChildren();
                this.brain.Stop();
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                this.LogInfo("Dumping status");
                this.brain.LogStatus();
                SituationOrchestrator.LogStatus();
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                this.LogInfo("Dumping situations");
                SituationLogger.LogSituations();
            }

            if (!this.isRunning)
            {
                return;
            }

            this.UpdateChildren();
        }

        /// <summary>
        /// Log an info-level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogInfo(string message)
        {
            Dispatcher.RunOnMainThread(() => this.Logger.LogInfo(message));
        }

        /// <summary>
        /// Log a trace-level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogTrace(string message)
        {
            Dispatcher.RunOnMainThread(() => this.Logger.LogInfo(message));
        }

        /// <summary>
        /// Log a warning-level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogWarn(string message)
        {
            Dispatcher.RunOnMainThread(() =>
            {
                this.Logger.LogWarning(message);
                GameAPI.Notify("Autoccultist Warning", message);
            });
        }

        /// <summary>
        /// Log and handle a fatal event.
        /// This will also stop the brain from running.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Fatal(string message)
        {
            Dispatcher.RunOnMainThread(() =>
            {
                this.Logger.LogError("Fatal - " + message);
                GameAPI.Notify("Autoccultist Fatal", message);
                this.isRunning = false;
                this.brain.Stop();
            });
        }

        private BrainConfig LoadBrainConfig()
        {
            var configPath = Path.Combine(AssemblyDirectory, "brain.yml");
            this.LogInfo(string.Format("Loading config from {0}", configPath));
            return BrainConfig.Load(configPath);
        }

        private void UpdateChildren()
        {
            this.brain.Update();
            SituationOrchestrator.Update();
            AutoccultistActor.Update();
        }
    }
}
