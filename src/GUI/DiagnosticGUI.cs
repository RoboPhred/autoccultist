namespace Autoccultist.GUI
{
    using System;
    using System.Linq;
    using Autoccultist.Brain;
    using Autoccultist.Config;
    using UnityEngine;

    /// <summary>
    /// GUI for general diagnostics.
    /// </summary>
    public static class DiagnosticGUI
    {
        private static readonly Lazy<int> WindowId = new Lazy<int>(() => GUIUtility.GetControlID(FocusType.Passive));

        /// <summary>
        /// Gets or sets a value indicating whether the window is being shown.
        /// </summary>
        public static bool IsShowing { get; set; }

        public static float Width => Mathf.Min(Screen.width, 350);

        public static float Height => Mathf.Min(Screen.height, 500);

        public static float OffsetX => Screen.width - Width - 10;

        /// <summary>
        /// Draw gui.
        /// </summary>
        public static void OnGUI()
        {
            if (!IsShowing)
            {
                return;
            }

            var height = Mathf.Min(Screen.height, 500);
            GUILayout.Window(WindowId.Value, new Rect(OffsetX, 10, Width, Height), DiagnosticsWindow, "Autoccultist Diagnostics");
        }

        private static void DiagnosticsWindow(int id)
        {
            if (Library.ParseErrors.Count > 0)
            {
                GUILayout.Label($"{Library.ParseErrors.Count} parse errors.");
                if (GUILayout.Button("Toggle Errors"))
                {
                    ParseErrorsGUI.IsShowing = !ParseErrorsGUI.IsShowing;
                }
            }

            if (GUILayout.Button("Reload All Configs"))
            {
                AutoccultistPlugin.Instance.ReloadAll();
            }

            GUILayout.BeginHorizontal();
            var mechHeart = GUILayout.Toggle(MechanicalHeart.IsRunning, "Mechanical Heart");
            if (mechHeart != MechanicalHeart.IsRunning)
            {
                if (mechHeart)
                {
                    MechanicalHeart.Start();
                }
                else
                {
                    MechanicalHeart.Stop();
                }
            }

            if (GUILayout.Button("Trigger Heartbeat"))
            {
                MechanicalHeart.Step();
            }

            GUILayout.EndHorizontal();

            var taskRunner = GUILayout.Toggle(Ego.IsRunning, "Ego");
            if (taskRunner != Ego.IsRunning)
            {
                if (taskRunner)
                {
                    Ego.Start();
                }
                else
                {
                    Ego.Stop();
                }
            }

            GUILayout.Label("Current Motivation: " + (Ego.CurrentMotivation != null ? Ego.CurrentMotivation.Name : "<None>"));

            GUILayout.BeginHorizontal();

            GUILayout.Label("Current Goals:");
            if (GUILayout.Button("Toggle Goals Menu"))
            {
                GoalsGUI.IsShowing = !GoalsGUI.IsShowing;
            }

            GUILayout.EndHorizontal();

            foreach (var goal in NucleusAccumbens.CurrentGoals)
            {
                var prefix = string.Empty;
                if (Ego.CurrentMotivation?.PrimaryGoals.Contains(goal) == true)
                {
                    prefix = "[Primary]";
                }
                else if (Ego.CurrentMotivation?.SupportingGoals.Contains(goal) == true)
                {
                    prefix = "[Supporting]";
                }
                else
                {
                    prefix = "[Custom]";
                }

                GUILayout.Label($"{prefix} {goal.Name}");
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
