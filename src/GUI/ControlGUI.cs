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
                ImmediateSynchronizationContext.Run(() =>
                {
                    MechanicalHeart.Step();
                });
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Diagnostics"))
            {
                DiagnosticsGUI.IsShowing = !DiagnosticsGUI.IsShowing;
            }

            if (GUILayout.Button("Performance"))
            {
                PerformanceGUI.IsShowing = !PerformanceGUI.IsShowing;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            var currentArc = NucleusAccumbens.CurrentImperatives.OfType<IArc>().FirstOrDefault();
            GUILayout.Label($"Current Arc: {currentArc?.Name ?? "None"}");

            if (currentArc == null)
            {
                if (GUILayout.Button("Start Arc", GUILayout.ExpandWidth(false)))
                {
                    ArcsGUI.IsShowing = !ArcsGUI.IsShowing;
                }
            }
            else
            {
                if (GUILayout.Button("Abort Arc", GUILayout.ExpandWidth(false)))
                {
                    NucleusAccumbens.RemoveImperative(currentArc);
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Current Goals:");
            if (GUILayout.Button("Goals"))
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

            GUILayout.Label("Current Reactions:");

            foreach (var reaction in NucleusAccumbens.CurrentReactions)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(reaction.ToString(), GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Abort", GUILayout.ExpandWidth(false)))
                {
                    reaction.Abort();
                }

                if (reaction is OperationReaction operationReaction)
                {
                    if (GUILayout.Button("Debug", GUILayout.ExpandWidth(false)))
                    {
                        var debug = operationReaction.DebugCurrentRecipe();
                        var state = GameStateProvider.Current.Situations.FirstOrDefault(x => x.SituationId == operationReaction.Operation.Situation);
                        DiagnosticsGUI.Show(debug + "\n\n" + operationReaction.Operation.DebugRecipes(state));
                    }
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}
