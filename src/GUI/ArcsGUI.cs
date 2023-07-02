namespace AutoccultistNS.GUI
{
    using System;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config;
    using UnityEngine;

    /// <summary>
    /// GUI for arcs.
    /// </summary>
    public static class ArcsGUI
    {
        private static readonly Lazy<int> WindowId = new(() => WindowManager.GetNextWindowID());

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

            var currentArc = NucleusAccumbens.CurrentImperatives.OfType<IArc>().FirstOrDefault();
            GUILayout.Label($"Current Arc: {currentArc?.Name ?? "None"}");

            GUILayout.Label("Available Arcs");

            foreach (var arc in Library.Arcs)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Activate", GUILayout.ExpandWidth(false)))
                {
                    if (currentArc != null)
                    {
                        NucleusAccumbens.RemoveImperative(currentArc);
                    }

                    NucleusAccumbens.AddImperative(arc);
                }

                GUILayout.Label(arc.Name);

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
