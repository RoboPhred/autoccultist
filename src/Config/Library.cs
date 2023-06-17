namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.IO;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    /// <summary>
    /// Library of various config assets.
    /// </summary>
    public static class Library
    {
        private static readonly List<YamlFileException> LibraryParseErrors = new();
        private static readonly List<IGoal> LibraryGoals = new();
        private static readonly List<IArc> LibraryArcs = new();

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
        /// Gets the collection of loaded goals.
        /// </summary>
        public static IReadOnlyCollection<IGoal> Goals
        {
            get
            {
                return LibraryGoals.ToArray();
            }
        }

        /// <summary>
        /// Gets the collection of loaded arcs.
        /// </summary>
        public static IReadOnlyCollection<IArc> Arcs
        {
            get
            {
                return LibraryArcs.ToArray();
            }
        }

        public static string GoalsDirectory
        {
            get
            {
                return Path.Combine(Autoccultist.AssemblyDirectory, "goals");
            }
        }

        public static string ArcsDirectory
        {
            get
            {
                return Path.Combine(Autoccultist.AssemblyDirectory, "arcs");
            }
        }

        /// <summary>
        /// Load all library assets.
        /// </summary>
        public static void LoadAll()
        {
            LibraryParseErrors.Clear();
            LoadGoals();
            LoadArcs();
        }

        private static void LoadGoals()
        {
            LibraryGoals.Clear();
            FilesystemHelpers.WalkDirectory(GoalsDirectory, LoadGoal);
        }

        private static void LoadGoal(string filePath)
        {
            try
            {
                var goal = Deserializer.Deserialize<GoalConfig>(filePath);
                LibraryGoals.Add(goal);
            }
            catch (YamlFileException ex)
            {
                LibraryParseErrors.Add(ex);
            }
        }

        private static void LoadArcs()
        {
            LibraryArcs.Clear();
            FilesystemHelpers.WalkDirectory(ArcsDirectory, LoadArc);
        }

        private static void LoadArc(string filePath)
        {
            try
            {
                var arc = Deserializer.Deserialize<ArcConfig>(filePath);
                LibraryArcs.Add(arc);
            }
            catch (YamlFileException ex)
            {
                LibraryParseErrors.Add(ex);
            }
        }
    }
}
