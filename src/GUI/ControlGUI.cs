namespace Autoccultist.GUI
{
    using System;
    using System.Linq;
    using Autoccultist.Brain;
    using Autoccultist.Config;
    using UnityEngine;

    /// <summary>
    /// GUI for controlling autoccultist.
    /// </summary>
    public static class ControlGUI
    {
        private static readonly Lazy<int> WindowId = new(() => GUIUtility.GetControlID(FocusType.Passive));

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

            var rect = WindowManager.GetWindowRect(350, 500);
            Debug.Log("Control window rect: " + rect);
            GUILayout.Window(WindowId.Value, rect, ControlWindow, "Autoccultist Control");
        }

        private static void ControlWindow(int id)
        {
            if (GUILayout.Button("Reload All Configs"))
            {
                AutoccultistPlugin.Instance.ReloadAll();
            }

            if (Library.ParseErrors.Count > 0)
            {
                GUILayout.Label($"{Library.ParseErrors.Count} parse errors.");
                if (GUILayout.Button("Toggle Errors"))
                {
                    ParseErrorsGUI.IsShowing = !ParseErrorsGUI.IsShowing;
                }
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

            GUILayout.BeginHorizontal();

            GUILayout.Label($"Current Arc: {Superego.CurrentArc?.Name ?? "None"}");

            if (GUILayout.Button("Arcs", GUILayout.ExpandWidth(false)))
            {
                ArcsGUI.IsShowing = !ArcsGUI.IsShowing;
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

            GUILayout.BeginHorizontal();

            GUILayout.Label("Current SuperEgo Motivation: " + (Superego.CurrentMotivation != null ? Superego.CurrentMotivation.Name : "<None>"));
            if (GUILayout.Button("Skip"))
            {
                Superego.SkipCurrentMotivation();
            }

            GUILayout.EndHorizontal();

            if (Superego.CurrentMotivation != Ego.CurrentMotivation)
            {
                GUILayout.Label("Current Ego Motivation: " + (Ego.CurrentMotivation != null ? Ego.CurrentMotivation.Name : "<None>"));
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label("Current Goals:");
            if (GUILayout.Button("Toggle Goals Menu"))
            {
                GoalsGUI.IsShowing = !GoalsGUI.IsShowing;
            }

            GUILayout.EndHorizontal();

            foreach (var goal in NucleusAccumbens.CurrentGoals)
            {
                var prefix = "[Custom]";
                if (Ego.CurrentMotivation?.PrimaryGoals.Contains(goal) == true)
                {
                    prefix = "[Primary]";
                }
                else if (Ego.CurrentMotivation?.SupportingGoals.Contains(goal) == true)
                {
                    prefix = "[Supporting]";
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
