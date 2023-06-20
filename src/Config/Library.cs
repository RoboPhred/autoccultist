namespace AutoccultistNS.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Yaml;

    /// <summary>
    /// Library of various config assets.
    /// </summary>
    public static class Library
    {
        private static readonly List<YamlFileException> LibraryParseErrors = new();
        private static readonly List<GoalConfig> LibraryGoals = new();
        private static readonly List<IArcConfig> LibraryArcs = new();

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
        /// Gets a library config object by file path.
        /// </summary>
        public static T GetByFilePath<T>(string filePath)
            where T : IConfigObject
        {
            return (T)GetByFilePath(typeof(T), filePath);
        }

        public static object GetByFilePath(Type type, string filePath)
        {
            var candidates = new List<IConfigObject>();

            candidates.AddRange(LibraryGoals.Where(x => type.IsAssignableFrom(x.GetType())));
            candidates.AddRange(LibraryArcs.Where(x => type.IsAssignableFrom(x.GetType())));

            var result = candidates.Find(candidate => candidate.FilePath == filePath);

            return result;
        }

        /// <summary>
        /// Gets a library config object by ID.
        /// </summary>
        public static T GetById<T>(string id)
            where T : IConfigObject
        {
            return (T)GetById(typeof(T), id);
        }

        public static IConfigObject GetById(Type type, string id)
        {
            var candidates = new List<IConfigObject>();

            candidates.AddRange(LibraryGoals.Where(x => type.IsAssignableFrom(x.GetType())));
            candidates.AddRange(LibraryArcs.Where(x => type.IsAssignableFrom(x.GetType())));

            var result = candidates.Find(candidate => candidate.Id == id);

            return result;
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
                NoonUtility.LogWarning(ex.ToString());
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
                var arc = Deserializer.Deserialize<MotivationalArcConfig>(filePath);
                LibraryArcs.Add(arc);
            }
            catch (YamlFileException ex)
            {
                LibraryParseErrors.Add(ex);
                NoonUtility.LogWarning(ex.ToString());
            }
        }
    }
}
