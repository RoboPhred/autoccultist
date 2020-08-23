namespace Autoccultist.Config
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Autoccultist.Yaml;

    /// <summary>
    /// Library of various config assets.
    /// </summary>
    public static class Library
    {
        private static readonly List<YamlFileException> LibraryParseErrors = new List<YamlFileException>();
        private static readonly Dictionary<string, GoalConfig> LibraryGoals = new Dictionary<string, GoalConfig>();

        /// <summary>
        /// Gets a collection of errors encountered while loading the library.
        /// </summary>
        public static IReadOnlyCollection<YamlFileException> ParseErrors
        {
            get
            {
                return LibraryParseErrors.ToArray();
            }
        }

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

        /// <summary>
        /// Gets the singular brain.
        /// </summary>
        // TODO: Should load multiple of these from a folder and show a ui for choosing which to use.
        public static BrainConfig Brain { get; private set; }

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
            LibraryParseErrors.Clear();
            LoadGoals();
            LoadBrain(Path.Combine(AutoccultistPlugin.AssemblyDirectory, "brain.yml"));
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
            catch (YamlFileException ex)
            {
                LibraryParseErrors.Add(ex);
            }
        }

        private static void LoadBrain(string filePath)
        {
            try
            {
                Brain = Deserializer.Deserialize<BrainConfig>(filePath);
            }
            catch (YamlFileException ex)
            {
                LibraryParseErrors.Add(ex);
            }
        }
    }
}
