namespace Autoccultist
{
    using System.IO;
    using Autoccultist.Brain;
    using Autoccultist.Config;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// The main entrypoint for Autoccultist, loaded by BenInEx.
    /// </summary>
    [BepInEx.BepInPlugin("net.robophreddev.CultistSimulator.Autoccultist", "Autoccultist", "0.0.1")]
    public class AutoccultistPlugin : BepInEx.BaseUnityPlugin
    {
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

            var harmony = new Harmony("net.robophreddev.CultistSimulator.Autoccultist");
            harmony.PatchAll();

            GameAPI.Initialize();

            var config = this.LoadBrainConfig();
            TaskDriver.SetTasks(config.Goals);

            this.LogInfo("Autoccultist initialized.");
        }

        /// <summary>
        /// Runs an update tick on the mod.
        /// </summary>
        public void Update()
        {
            MechanicalHeart.Update();
            this.HandleHotkeys();
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
            this.StopAutoccultist();
        }

        private void HandleHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                if (MechanicalHeart.IsRunning)
                {
                    this.LogInfo("Stopping Autoccultist");
                    this.StopAutoccultist();
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        this.LogInfo("Reloading tasks");
                        var config = this.LoadBrainConfig();
                        TaskDriver.SetTasks(config.Goals);
                    }
                    else
                    {
                        this.LogInfo("Starting Autoccultist");
                        this.StartAutoccultist();
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                this.LogInfo("Dumping status");
                GoalDriver.DumpStatus();
                SituationOrchestrator.LogStatus();
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                this.LogInfo("Dumping situations");
                SituationLogger.LogSituations();
            }
        }

        private void StartAutoccultist()
        {
            TaskDriver.Start();
            MechanicalHeart.Start();
        }

        private void StopAutoccultist()
        {
            MechanicalHeart.Stop();
            TaskDriver.Stop();
            GoalDriver.Reset();
        }

        private BrainConfig LoadBrainConfig()
        {
            var configPath = Path.Combine(AssemblyDirectory, "brain.yml");
            this.LogInfo(string.Format("Loading config from {0}", configPath));
            return BrainConfig.Load(configPath);
        }
    }
}
