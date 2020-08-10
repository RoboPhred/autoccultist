using Autoccultist.Brain;
using Autoccultist.Brain.Config;
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

        void Start()
        {
            Instance = this;

            var config = this.LoadConfig();
            this.brain = new AutoccultistBrain(config);

            this.Info("Autoccultist initialized.");
        }

        BrainConfig LoadConfig()
        {
            var binPath = this.GetType().Assembly.Location;
            binPath = System.IO.Path.GetDirectoryName(binPath);
            var configPath = System.IO.Path.Combine(binPath, "brain.yml");
            this.Info(string.Format("Loading config from {0}", configPath));
            return BrainConfig.Load(configPath);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                if (this.isRunning)
                {
                    this.Info("Stopping brain");
                    this.brain.Stop();
                    this.isRunning = false;
                }
                else
                {
                    this.Info("Starting brain");
                    this.brain.Start();
                    this.isRunning = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.F10))
            {
                // Ensure not running
                this.isRunning = false;
                this.Info("Step");
                this.brain.Start();
                GameAPI.DoHeartbeat();
                this.brain.Stop();

            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                this.Info("Dumping status");
                this.brain.LogStatus();
            }

            if (!this.isRunning)
            {
                return;
            }
            GameAPI.DoHeartbeat();
        }

        public void Info(string message)
        {
            this.Logger.LogInfo(message);
        }

        public void Trace(string message)
        {
            this.Logger.LogInfo(message);
        }

        public void Warn(string message)
        {
            this.Logger.LogWarning(message);
            GameAPI.Notify("Autoccultist Warning", message);
        }

        public void Fatal(string message)
        {
            this.Logger.LogError("Fatal - " + message);
            GameAPI.Notify("Autoccultist Fatal", message);
            this.isRunning = false;
            this.brain.Stop();
        }
    }
}