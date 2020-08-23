namespace Autoccultist.GUI
{
    using System;
    using Autoccultist.Config;
    using Autoccultist.Yaml;
    using UnityEngine;
    using YamlDotNet.Core;

    /// <summary>
    /// GUI for listing and activating goals.
    /// </summary>
    public static class ParseErrorsGUI
    {
        private static readonly Lazy<int> WindowId = new Lazy<int>(() => GUIUtility.GetControlID(FocusType.Passive));

        private static Vector2 errorsScrollPosition = default;

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
            GUILayout.Window(WindowId.Value, new Rect(offsetX, offsetY, width, height), ParseErrorsWindow, "Autoccultist Parse Errors");
        }

        private static void ParseErrorsWindow(int id)
        {
            errorsScrollPosition = GUILayout.BeginScrollView(errorsScrollPosition, GUILayout.Height(600));

            foreach (var ex in Library.ParseErrors)
            {
                GUILayout.Label($"{FilesystemHelpers.GetRelativePath(ex.FilePath, AutoccultistPlugin.AssemblyDirectory)}:{ex.Start.Line}");
                GUILayout.Label(ex.GetInnermostMessage());
                if (GUILayout.Button("Copy to clipboard"))
                {
                    TextEditor textEditor = new TextEditor { text = ex.ToString() };

                    textEditor.SelectAll();
                    textEditor.Copy();
                }
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Close"))
            {
                IsShowing = false;
            }
        }
    }
}
