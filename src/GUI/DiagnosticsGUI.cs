namespace AutoccultistNS.GUI
{
    using System;
    using AutoccultistNS.Brain;
    using UnityEngine;

    /// <summary>
    /// Provides a GUI for inspecting the state of autoccultist.
    /// </summary>
    public static class DiagnosticsGUI
    {
        private static readonly Lazy<int> WindowId = new(() => WindowManager.GetNextWindowID());

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

            GUILayout.Window(WindowId.Value, WindowManager.GetWindowRect(350, 1000), DiagnosticsWindow, "Autoccultist Diagnostics");
        }

        public static void Show(string content)
        {
            IsShowing = true;
            DiagnosticsGUI.content = content;
        }

        private static void DiagnosticsWindow(int id)
        {
            // FIXME: Reimplement this
            if (GUILayout.Button("Dump Nucleus Accumbens"))
            {
                content = NucleusAccumbens.DumpStatus();
            }

            if (GUILayout.Button("Dump Situations"))
            {
                content = SituationLogger.DumpSituations();
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (content != null)
            {
                GUILayout.TextArea(content);
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Copy to clipboard") && !string.IsNullOrEmpty(content))
            {
                var textEditor = new TextEditor { text = content };

                textEditor.SelectAll();
                textEditor.Copy();
            }

            if (GUILayout.Button("Close"))
            {
                IsShowing = false;
            }
        }
    }
}
