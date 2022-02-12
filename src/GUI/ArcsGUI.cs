namespace Autoccultist.GUI
{
    using System;
    using System.Linq;
    using Autoccultist.Brain;
    using Autoccultist.Config;
    using Autoccultist.GameState;
    using UnityEngine;

    /// <summary>
    /// GUI for arcs.
    /// </summary>
    public static class ArcsGUI
    {
        private static readonly Lazy<int> WindowId = new(() => GUIUtility.GetControlID(FocusType.Passive));

        private static Vector2 scrollPosition = default;

        /// <summary>
        /// Gets or sets a value indicating whether the arcs gui is showing.
        /// </summary>
        public static bool IsShowing { get; set; }

        /// <summary>
        /// Draw the GUI.
        /// </summary>
        public static void OnGUI()
        {
            if (!IsShowing)
            {
                return;
            }

            GUILayout.Window(WindowId.Value, WindowManager.GetWindowRect(500, 900), ArcsWindow, "Autoccultist Arcs");
        }

        private static void ArcsWindow(int id)
        {
            GUILayout.Label("Arcs");

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.BeginHorizontal();

            GUILayout.Label($"Current Arc: {Superego.CurrentArc?.Name ?? "None"}");

            if (GUILayout.Button("Autodetect", GUILayout.ExpandWidth(false)))
            {
                Superego.AutoselectArc();
            }

            GUILayout.EndHorizontal();

            GUILayout.Label("Available Arcs");

            foreach (var arc in Library.Arcs)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(arc.Name);

                if (GUILayout.Button("Activate", GUILayout.ExpandWidth(false)))
                {
                    Superego.SetArc(arc);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}
