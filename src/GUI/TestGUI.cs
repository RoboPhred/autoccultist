namespace Autoccultist.GUI
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Test GUI.
    /// </summary>
    public static class TestGUI
    {
        private static readonly Lazy<int> WindowId = new Lazy<int>(() => GUIUtility.GetControlID(FocusType.Passive));

        /// <summary>
        /// Draw the test gui.
        /// </summary>
        public static void OnGUI()
        {
            var width = Mathf.Min(Screen.width, 350);
            var height = Mathf.Min(Screen.height, 500);
            var offsetX = Screen.width - width - 10;
            var offsetY = 10;
            GUILayout.Window(WindowId.Value, new Rect(offsetX, offsetY, width, height), TestWindow, "Autoccultist Test");
        }

        private static void TestWindow(int id)
        {
            var value = GUILayout.Toggle(AutoccultistPlugin.Instance.IsRunning, "Running");
            if (value != AutoccultistPlugin.Instance.IsRunning)
            {
                if (value)
                {
                    AutoccultistPlugin.Instance.StartBrain();
                }
                else
                {
                    AutoccultistPlugin.Instance.StopBrain();
                }
            }

            if (GUILayout.Button("Reload Config"))
            {
                AutoccultistPlugin.Instance.ResetBrain();
            }

            GUILayout.Label("Current Goal: " + AutoccultistPlugin.Instance.Brain.CurrentGoal?.Name ?? "[none]");
        }
    }
}
