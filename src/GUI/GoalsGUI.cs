namespace AutoccultistNS.GUI
{
    using System;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config;
    using AutoccultistNS.GameState;
    using UnityEngine;

    /// <summary>
    /// GUI for listing and activating goals.
    /// </summary>
    public static class GoalsGUI
    {
        private static readonly Lazy<int> WindowId = new(() => WindowManager.GetNextWindowID());

        private static Vector2 scrollPositionCurrentGoals = default;
        private static Vector2 scrollPositionNewGoals = default;
        private static Vector2 scrollPositionSatisfiedGoals = default;

        private static string searchFilter = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the Goals gui is being shown.
        /// </summary>
        public static bool IsShowing { get; set; }

        /// <summary>
        /// Draw the gui.
        /// </summary>
        public static void OnGUI()
        {
            if (!IsShowing)
            {
                return;
            }

            GUILayout.Window(WindowId.Value, WindowManager.GetWindowRect(500, 900), GoalsWindow, "Autoccultist Goals");
        }

        private static void GoalsWindow(int id)
        {
            GUILayout.Label("Current Goals");

            scrollPositionCurrentGoals = GUILayout.BeginScrollView(scrollPositionCurrentGoals);

            foreach (var goal in NucleusAccumbens.CurrentGoals)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Cancel", GUILayout.Width(75)))
                {
                    NucleusAccumbens.RemoveGoal(goal);
                }

                GUILayout.Label(goal.Name, GUILayout.ExpandWidth(false));

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.Label("Available Goals");

            searchFilter = GUILayout.TextField(searchFilter, GUILayout.ExpandWidth(true)).ToLower();

            scrollPositionNewGoals = GUILayout.BeginScrollView(scrollPositionNewGoals);

            foreach (var goal in Library.Goals.Where(x => searchFilter == string.Empty || x.Name.ToLower().Contains(searchFilter)))
            {
                if (goal.IsSatisfied(GameStateProvider.Current) || NucleusAccumbens.CurrentGoals.Contains(goal))
                {
                    continue;
                }

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Activate", GUILayout.Width(75)))
                {
                    NucleusAccumbens.AddGoal(goal);
                }

                GUILayout.Label(goal.Name, GUILayout.ExpandWidth(false));

                if (goal.CanActivate(GameStateProvider.Current))
                {
                    GUILayout.Label("[CanActivate]", GUILayout.ExpandWidth(false));
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            scrollPositionSatisfiedGoals = GUILayout.BeginScrollView(scrollPositionSatisfiedGoals);

            GUILayout.Label("Satisfied Goals");
            foreach (var goal in Library.Goals)
            {
                if (!goal.IsSatisfied(GameStateProvider.Current))
                {
                    continue;
                }

                GUILayout.Label(goal.Name, GUILayout.ExpandWidth(false));
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Close"))
            {
                IsShowing = false;
            }
        }
    }
}
