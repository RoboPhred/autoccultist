namespace Autoccultist.GUI
{
    using System;
    using System.Linq;
    using Autoccultist.Brain;
    using Autoccultist.Config;
    using Autoccultist.GameState;
    using UnityEngine;

    /// <summary>
    /// GUI for listing and activating goals.
    /// </summary>
    public static class GoalsGUI
    {
        private static readonly Lazy<int> WindowId = new Lazy<int>(() => GUIUtility.GetControlID(FocusType.Passive));

        private static Vector2 currentGoalsScrollPosition = default;
        private static Vector2 availableGoalsScrollPosition = default;

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

            var width = Mathf.Min(Screen.width, 500);
            var height = Mathf.Min(Screen.height, 700);
            var offsetX = (Screen.width * 3 / 4) - (width / 2);
            var offsetY = 10;
            GUILayout.Window(WindowId.Value, new Rect(offsetX, offsetY, width, height), GoalsWindow, "Autoccultist Goals");
        }

        private static void GoalsWindow(int id)
        {
            GUILayout.Label("Current Goals");
            currentGoalsScrollPosition = GUILayout.BeginScrollView(currentGoalsScrollPosition, GUILayout.Height(100));

            foreach (var goal in GoalDriver.CurrentGoals)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Cancel", GUILayout.Width(75)))
                {
                    GoalDriver.RemoveGoal(goal);
                }

                GUILayout.Label(goal.Name, GUILayout.ExpandWidth(false));

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.Label("Available Goals");
            availableGoalsScrollPosition = GUILayout.BeginScrollView(availableGoalsScrollPosition, GUILayout.Height(450));

            foreach (var goal in Library.Goals)
            {
                GUILayout.BeginHorizontal();

                GUI.enabled = !GoalDriver.CurrentGoals.Contains(goal);
                if (GUILayout.Button("Activate", GUILayout.Width(75)))
                {
                    GoalDriver.AddGoal(goal);
                }

                GUI.enabled = true;

                GUILayout.Label(goal.Name, GUILayout.ExpandWidth(false));

                if (goal.CanActivate(GameStateProvider.Current))
                {
                    GUILayout.Label("[CanActivate]", GUILayout.ExpandWidth(false));
                }

                if (goal.IsSatisfied(GameStateProvider.Current))
                {
                    GUILayout.Label("[IsSatisfied]", GUILayout.ExpandWidth(false));
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Close"))
            {
                IsShowing = false;
            }
        }
    }
}
