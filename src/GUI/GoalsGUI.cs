namespace AutoccultistNS.GUI
{
    using System;
    using System.IO;
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

        private static Vector2 scrollPositionNewGoals = default;

        private static string searchFilter = string.Empty;

        private static string folderFilter = string.Empty;

        private static bool filterCanActivate = true;

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

        private static bool FilterGoal(IGoal goal, bool exactFolder)
        {
            if (!goal.Name.ToLower().Contains(searchFilter))
            {
                return false;
            }

            var path = goal.GetLibraryPath();
            if (path == null && !string.IsNullOrEmpty(folderFilter))
            {
                return false;
            }

            if (exactFolder)
            {
                if (path == null)
                {
                    return false;
                }

                var endOfFolder = path.LastIndexOf(Path.DirectorySeparatorChar);
                if (endOfFolder == -1)
                {
                    if (!string.IsNullOrEmpty(folderFilter))
                    {
                        return false;
                    }
                }
                else
                {
                    var fullPath = path.Substring(0, endOfFolder + 1);

                    if (fullPath.ToLower() != folderFilter.ToLower())
                    {
                        return false;
                    }
                }
            }
            else if (!path.ToLower().StartsWith(folderFilter.ToLower()))
            {
                return false;
            }

            return true;
        }

        private static void GoalsWindow(int id)
        {
            searchFilter = GUILayout.TextField(searchFilter, GUILayout.ExpandWidth(true)).ToLower();
            filterCanActivate = GUILayout.Toggle(filterCanActivate, "Can Activate");

            scrollPositionNewGoals = GUILayout.BeginScrollView(scrollPositionNewGoals);

            GUILayout.Label(string.IsNullOrEmpty(folderFilter) ? "/" : folderFilter);

            if (!string.IsNullOrEmpty(folderFilter))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);

                if (GUILayout.Button("../"))
                {
                    var index = folderFilter.Substring(0, folderFilter.Length - 1).LastIndexOf(Path.DirectorySeparatorChar);
                    if (index == -1)
                    {
                        folderFilter = string.Empty;
                    }
                    else
                    {
                        folderFilter = folderFilter.Substring(0, index + 1);
                    }
                }

                GUILayout.EndHorizontal();
            }

            var folders =
                from goal in Library.Goals
                where FilterGoal(goal, false)
                let path = goal.GetLibraryPath()
                where path != null
                let relative = path.Substring(folderFilter.Length)
                let split = relative.IndexOf(Path.DirectorySeparatorChar)
                where split != -1
                let folder = relative.Substring(0, split)
                group goal by folder.ToLower() into g
                orderby g.Key
                select g.Key;

            foreach (var folder in folders)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Space(20);

                if (GUILayout.Button(folder))
                {
                    folderFilter = folderFilter + folder + Path.DirectorySeparatorChar;
                }

                GUILayout.EndHorizontal();
            }

            var goals =
                from goal in Library.Goals
                where FilterGoal(goal, string.IsNullOrEmpty(searchFilter))
                let satisfied = goal.IsSatisfied(GameStateProvider.Current)
                let canActivate = goal.CanActivate(GameStateProvider.Current)
                let active = NucleusAccumbens.CurrentGoals.Contains(goal)
                where filterCanActivate == false || canActivate
                orderby goal.Name.ToLower()
                select new { Goal = goal, Satisfied = satisfied, CanActivate = canActivate, Active = active };

            foreach (var pair in goals)
            {
                var goal = pair.Goal;

                GUILayout.BeginHorizontal();

                GUILayout.Space(20);

                var prepend = string.Empty;
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    // When searching, we show deep folders.  So show the path
                    var path = goal.GetLibraryPath();
                    var lastMarker = path.LastIndexOf(Path.DirectorySeparatorChar);
                    path = path.Substring(0, lastMarker + 1);
                    path = path.Substring(folderFilter.Length);
                    prepend = path;
                }

                GUILayout.Label(prepend + goal.Name, GUILayout.ExpandWidth(false));

                GUILayout.Label(string.Empty, GUILayout.ExpandWidth(true));

                if (pair.Active)
                {
                    GUILayout.Label("[Active]");
                }
                else if (pair.CanActivate)
                {
                    if (GUILayout.Button("Activate"))
                    {
                        NucleusAccumbens.AddGoal(goal);
                    }
                }
                else if (pair.Satisfied)
                {
                    GUILayout.Label("[Satisfied]");
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
