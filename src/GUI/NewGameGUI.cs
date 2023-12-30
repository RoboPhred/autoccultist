namespace AutoccultistNS.GUI
{
    using System;
    using System.Linq;
    using AutoccultistNS.Config;
    using UnityEngine;

    /// <summary>
    /// GUI for controlling autoccultist.
    /// </summary>
    public static class NewGameGUI
    {
        private static readonly Lazy<int> WindowId = new(() => WindowManager.GetNextWindowID());

        private static Vector2 scrollPosition = default;
        private static string searchText = string.Empty;

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

            GUILayout.Window(WindowId.Value, WindowManager.GetWindowRect(350, 500), ControlWindow, "New Automated Game");
        }

        private static void ControlWindow(int id)
        {
            searchText = GUILayout.TextField(searchText);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var arc in Library.Arcs.Where(a => a.SupportsNewGame && a.Name.ToLower().Contains(searchText.ToLower())))
            {
                if (GUILayout.Button(arc.Name))
                {
                    Autoccultist.Instance.StartNewGame(arc);
                    IsShowing = false;
                }
            }

            GUILayout.EndScrollView();
        }
    }
}
