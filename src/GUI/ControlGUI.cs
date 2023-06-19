namespace AutoccultistNS.GUI
{
    using System;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config;
    using AutoccultistNS.GameState;
    using UnityEngine;

    /// <summary>
    /// GUI for controlling autoccultist.
    /// </summary>
    public static class ControlGUI
    {
        private static readonly Lazy<int> WindowId = new(() => WindowManager.GetNextWindowID());

        /// <summary>
        /// Gets or sets a value indicating whether the window is being shown.
        /// </summary>
        public static bool IsShowing { get; set; }

        /// <summary>
        /// Draw gui.
        /// </summary>
        public static void OnGUI()
        {
            if (!IsShowing)
            {
                return;
            }

            GUILayout.Window(WindowId.Value, WindowManager.GetWindowRect(350, 500), ControlWindow, "Autoccultist Control");
        }

        private static void ControlWindow(int id)
        {
            if (GUILayout.Button("Reload All Configs"))
            {
                Autoccultist.Instance.ReloadAll();
            }

            if (Library.ParseErrors.Count > 0)
            {
                GUILayout.Label($"{Library.ParseErrors.Count} parse errors.");
                if (GUILayout.Button("Toggle Errors"))
                {
                    ParseErrorsGUI.IsShowing = !ParseErrorsGUI.IsShowing;
                }
            }

            if (GUILayout.Button("New Game"))
            {
                NewGameGUI.IsShowing = !NewGameGUI.IsShowing;
            }

            if (!GameAPI.IsRunning)
            {
                GUILayout.Label("Game is not running.");
                return;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(MechanicalHeart.IsRunning ? "Pause" : "Run"))
            {
                if (MechanicalHeart.IsRunning)
                {
                    MechanicalHeart.Stop();
                }
                else
                {
                    MechanicalHeart.Start();
                }
            }

            if (GUILayout.Button("Heartbeat"))
            {
                MechanicalHeart.Step();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            var currentArc = NucleusAccumbens.CurrentImperatives.OfType<IArc>().FirstOrDefault();
            GUILayout.Label($"Current Arc: {currentArc?.Name ?? "None"}");

            if (GUILayout.Button("Arcs", GUILayout.ExpandWidth(false)))
            {
                ArcsGUI.IsShowing = !ArcsGUI.IsShowing;
            }

            GUILayout.EndHorizontal();

            // TODO: Do we want to keep visibility into motivations?
            // GUILayout.BeginHorizontal();

            // GUILayout.Label("Current Motivation: " + (Superego.CurrentMotivation != null ? Superego.CurrentMotivation.Name : "<None>"));
            // if (GUILayout.Button("Skip"))
            // {
            //     Superego.SkipCurrentMotivation();
            // }

            // GUILayout.EndHorizontal();

            if (GUILayout.Button("Diagnostics"))
            {
                DiagnosticsGUI.IsShowing = !DiagnosticsGUI.IsShowing;
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label("Current Goals:");
            if (GUILayout.Button("Goals Menu"))
            {
                GoalsGUI.IsShowing = !GoalsGUI.IsShowing;
            }

            GUILayout.EndHorizontal();

            foreach (var imperative in NucleusAccumbens.CurrentImperatives)
            {
                foreach (var goal in imperative.DescribeCurrentGoals(GameStateProvider.Current))
                {
                    GUILayout.Label(goal);
                }
            }

            GUILayout.Label("Current Orchestrations:");

            foreach (var entry in SituationOrchestrator.CurrentOrchestrations)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{entry.Key}: {entry.Value}", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Abort", GUILayout.ExpandWidth(false)))
                {
                    entry.Value.Abort();
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}
