using System.IO;
using Autoccultist.Brain;
using Autoccultist.Brain.Config;
using Autoccultist.Actor;
using UnityEngine;

namespace Autoccultist
{
    [BepInEx.BepInPlugin("net.robophreddev.CultistSimulator.Autoccultist", "Autoccultist", "0.0.1")]
    public class AutoccultistPlugin : BepInEx.BaseUnityPlugin
    {
        private bool isRunning = false;

        private AutoccultistBrain brain;

        public static AutoccultistPlugin Instance
        {
            get;
            private set;
        }

        public static string AssemblyDirectory
        {
            get
            {
                var assemblyLocation = typeof(AutoccultistPlugin).Assembly.Location;
                var assemblyDir = Path.GetDirectoryName(assemblyLocation);
                return assemblyDir;
            }
        }

        void Start()
        {
            Instance = this;

            Dispatcher.Initialize();

            var brainConfig = this.LoadBrainConfig();
            LogInfo($"Loaded {brainConfig.Goals.Count} goals.");

            this.brain = new AutoccultistBrain(brainConfig);

            this.LogInfo("Autoccultist initialized.");
        }

        BrainConfig LoadBrainConfig()
        {
            var configPath = System.IO.Path.Combine(AssemblyDirectory, "brain.yml");
            this.LogInfo(string.Format("Loading config from {0}", configPath));
            return BrainConfig.Load(configPath);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                if (this.isRunning)
                {
                    this.LogInfo("Stopping brain");
                    this.brain.Stop();
                    this.isRunning = false;
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
                UpdateChildren();
                this.brain.Stop();

            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                this.LogInfo("Dumping status");
                this.brain.LogStatus();
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
            UpdateChildren();
        }

        public void LogInfo(string message)
        {
            Dispatcher.RunOnMainThread(() =>
            {
                this.Logger.LogInfo(message);
            });
        }

        public void LogTrace(string message)
        {
            Dispatcher.RunOnMainThread(() =>
            {
                this.Logger.LogInfo(message);
            });

        }

        public void LogWarn(string message)
        {
            Dispatcher.RunOnMainThread(() =>
            {
                this.Logger.LogWarning(message);
                GameAPI.Notify("Autoccultist Warning", message);
            });
        }

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

        private void UpdateChildren()
        {
            this.brain.Update();
            SituationOrchestrator.Update();
            AutoccultistActor.Update();
        }
    }
}