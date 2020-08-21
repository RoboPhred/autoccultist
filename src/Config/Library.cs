namespace Autoccultist.Config
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Autoccultist.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// Library of various config assets.
    /// </summary>
    public static class Library
    {
        private static readonly Dictionary<string, GoalConfig> LibraryGoals = new Dictionary<string, GoalConfig>();

        /// <summary>
        /// Gets a collection of goals loaded by the library.
        /// </summary>
        public static IReadOnlyCollection<GoalConfig> Goals
        {
            get
            {
                return LibraryGoals.Values.ToArray();
            }
        }

        private static string GoalsDirectory
        {
            get
            {
                return Path.Combine(AutoccultistPlugin.AssemblyDirectory, "goals");
            }
        }

        /// <summary>
        /// Load all library assets.
        /// </summary>
        public static void LoadAll()
        {
            LoadGoals();
        }

        private static void LoadGoals()
        {
            LibraryGoals.Clear();
            FilesystemHelpers.WalkDirectory(GoalsDirectory, LoadGoal);
        }

        private static void LoadGoal(string filePath)
        {
            var localPath = FilesystemHelpers.GetRelativePath(filePath, GoalsDirectory);
            try
            {
                var goal = Deserializer.Deserialize<GoalConfig>(filePath);
                LibraryGoals.Add(localPath, goal);
            }
            catch (YamlException)
            {
            }
        }
    }
}
