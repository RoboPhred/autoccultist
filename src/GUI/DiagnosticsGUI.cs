namespace Autoccultist.GUI
{
    using System;
    using Autoccultist.Brain;
    using UnityEngine;

    /// <summary>
    /// Provides a GUI for inspecting the state of autoccultist.
    /// </summary>
    public static class DiagnosticsGUI
    {
        private static readonly Lazy<int> WindowId = new(() => GUIUtility.GetControlID(FocusType.Passive));

        private static Vector2 scrollPosition = default;

        private static string content;

        /// <summary>
        /// Gets or sets a value indicating whether the window is being shown.
        /// </summary>
        public static bool IsShowing { get; set; }

        /// <summary>
        /// Renders the Diagnostics window.
        /// </summary>
        public static void OnGUI()
        {
            if (!IsShowing)
            {
                return;
            }

            GUILayout.Window(WindowId.Value, WindowManager.GetWindowRect(350, 500), DiagnosticsWindow, "Autoccultist Diagnostics");
        }

        private static void DiagnosticsWindow(int id)
        {
            if (GUILayout.Button("Dump Nucleus Accumbens"))
            {
                content = NucleusAccumbens.DumpStatus();
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (content != null)
            {
                GUILayout.TextArea(content);
            }

            GUILayout.EndScrollView();
        }
    }
}
