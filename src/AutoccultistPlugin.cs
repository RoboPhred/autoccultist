namespace Autoccultist
{
    using System;
    using System.IO;
    using System.Linq;
    using Autoccultist.Brain;
    using Autoccultist.Config;
    using Autoccultist.GameState;
    using Autoccultist.GUI;
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
                return Path.GetDirectoryName(assemblyLocation);
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

            this.ReloadAll();
            if (Library.ParseErrors.Count > 0)
            {
                ParseErrorsGUI.IsShowing = true;
            }

            GameEventSource.GameStarted += (_, __) => Superego.AutoselectArc();

            GameEventSource.GameEnded += (_, __) =>
            {
                this.StopAutoccultist();
                Superego.SetArc(null);
            };

            this.LogInfo("Autoccultist initialized.");
        }

        /// <summary>
        /// Reload all tasks in the TaskDriver.
        /// </summary>
        public void ReloadAll()
        {
            var previousArcName = Superego.CurrentArc?.Name;

            this.StopAutoccultist();
            this.LogInfo("Reloading all configs");
            Library.LoadAll();
            Superego.Clear();

            if (GameAPI.IsRunning)
            {
                var arc = Library.Arcs.FirstOrDefault(a => a.Name == previousArcName);
                if (arc != null)
                {
                    Superego.SetArc(arc);
                }
            }
        }

        /// <summary>
        /// Renders the mod GUI.
        /// </summary>
        public void OnGUI()
        {
            WindowManager.OnPreGUI();

            if (!GameAPI.IsRunning)
            {
                // Allow ParseErrorsGUI to run when the core game is not in play.
                ParseErrorsGUI.OnGUI();
                return;
            }

            ControlGUI.OnGUI();
            ParseErrorsGUI.OnGUI();
            DiagnosticsGUI.OnGUI();
            GoalsGUI.OnGUI();
            ArcsGUI.OnGUI();
        }

        /// <summary>
        /// Runs an update tick on the mod.
        /// </summary>
        public void Update()
        {
            GameStateProvider.Invalidate();
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
        /// Logs a warning-level message with an exception.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">The message.</param>
        public void LogWarn(Exception ex, string message)
        {
            this.Logger.LogWarning($"{message}\n{ex.Message}\n{ex.StackTrace}");
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
                        this.ReloadAll();
                    }
                    else
                    {
                        this.LogInfo("Starting Autoccultist");
                        this.StartAutoccultist();
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.F10))
            {
                ControlGUI.IsShowing = !ControlGUI.IsShowing;
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                this.LogInfo("Dumping situations");
                SituationLogger.LogSituations();
            }
        }

        private void StartAutoccultist()
        {
            Ego.Start();
            MechanicalHeart.Start();
        }

        private void StopAutoccultist()
        {
            MechanicalHeart.Stop();
            Ego.Stop();
            NucleusAccumbens.Reset();
            SituationOrchestrator.AbortAll();
        }
    }
}
