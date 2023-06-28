namespace AutoccultistNS.GUI
{
    using System;
    using AutoccultistNS.GameState;
    using UnityEngine;

    /// <summary>
    /// Provides a GUI for inspecting the state of autoccultist.
    /// </summary>
    public static class PerformanceGUI
    {
        private static readonly Lazy<int> WindowId = new(() => WindowManager.GetNextWindowID());

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

            GUILayout.Window(WindowId.Value, WindowManager.GetWindowRect(500, 100), PerformanceWindow, "Autoccultist Performance");
        }

        private static void PerformanceWindow(int id)
        {
            if (GUILayout.Button("Clear"))
            {
                PerfMonitor.ClearStatistics();
                CacheUtils.ClearStatistics();
            }

            GUILayout.Label($"Cache misses {CacheUtils.MissesPerSecond:0.000}ps hits {CacheUtils.HitsPerSecond:0.000}ps");

            var state = GameStateProvider.Current;
            GUILayout.Label($"GameState {state.GetHashCode()}");
            GUILayout.Label($"TabletopCards Hash {state.TabletopCards.GetHashCode()}");
            GUILayout.Label($"EnRouteCards Hash {state.EnRouteCards.GetHashCode()}");
            GUILayout.Label($"CodexCards Hash {state.CodexCards.GetHashCode()}");
            GUILayout.Label($"Situations Hash {state.Situations.GetHashCode()}");
            GUILayout.Label($"Mansus Hash {state.Mansus.GetHashCode()}");

            foreach (var entry in PerfMonitor.Entries)
            {
                GUILayout.Label($"{entry.Key}: {entry.Value.Average:0.000}ms {entry.Value.SamplesPerSecond:0}cps (max {entry.Value.Max:0.000}ms, >10 {entry.Value.GreaterThanAllocatedRate:0.000} per second)");
            }
        }
    }
}
