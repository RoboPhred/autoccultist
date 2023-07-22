namespace AutoccultistNS.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AutoccultistNS.Yaml;

    /// <summary>
    /// Library of various config assets.
    /// </summary>
    public static class Library
    {
        private static readonly List<YamlFileException> LibraryParseErrors = new();
        private static readonly List<GoalConfig> LibraryGoals = new();
        private static readonly List<IArcConfig> LibraryArcs = new();
        private static readonly List<OperationConfig> LibraryOperations = new();

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
        /// Gets the collection of loaded arcs.
        /// </summary>
        public static IReadOnlyCollection<IArcConfig> Arcs
        {
            get
            {
                return LibraryArcs.ToArray();
            }
        }

        /// <summary>
        /// Gets the collection of loaded goals.
        /// </summary>
        public static IReadOnlyCollection<IGoalConfig> Goals
        {
            get
            {
                return LibraryGoals.ToArray();
            }
        }

        /// <summary>
        /// Gets the collection of loaded operations.
        /// </summary>
        public static IReadOnlyCollection<OperationConfig> Operations
        {
            get
            {
                return LibraryOperations.ToArray();
            }
        }

        /// <summary>
        /// Gets the directory where arc assets are stored.
        /// </summary>
        public static string ArcsDirectory
        {
            get
            {
                return Path.Combine(Autoccultist.AssemblyDirectory, "arcs");
            }
        }

        /// <summary>
        /// Gets the directory where goal assets are stored.
        /// </summary>
        public static string GoalsDirectory
        {
            get
            {
                return Path.Combine(Autoccultist.AssemblyDirectory, "goals");
            }
        }

        /// <summary>
        /// Gets the directory where operation assets are stored.
        /// </summary>
        public static string OperationsDirectory
        {
            get
            {
                return Path.Combine(Autoccultist.AssemblyDirectory, "operations");
            }
        }

        /// <summary>
        /// Gets a library config object by file path.
        /// </summary>
        /// <typeparam name="T">The type of config object to get.</typeparam>
        /// <param name="filePath">The file path of the config object.</param>
        /// <returns>The config object, or null if not found.</returns>
        public static T GetByFilePath<T>(string filePath)
            where T : IConfigObject
        {
            return (T)GetByFilePath(typeof(T), filePath);
        }

        /// <summary>
        /// Gets a library config object by file path.
        /// </summary>
        /// <param name="type">The type of config object to get.</param>
        /// <param name="filePath">The file path of the config object.</param>
        /// <returns>The config object, or null if not found.</returns>
        public static object GetByFilePath(Type type, string filePath)
        {
            var candidates = new List<IConfigObject>();

            var result = GetAllConfigObjects(type).FirstOrDefault(candidate => candidate.FilePath == filePath);

            return result;
        }

        /// <summary>
        /// Gets a library config object by ID.
        /// </summary>
        /// <typeparam name="T">The type of config object to get.</typeparam>
        /// <param name="id">The ID of the config object.</param>
        /// <returns>The config object, or null if not found.</returns>
        public static T GetById<T>(string id)
            where T : IConfigObject
        {
            return (T)GetById(typeof(T), id);
        }

        /// <summary>
        /// Gets a library config object by ID.
        /// </summary>
        /// <param name="type">The type of config object to get.</param>
        /// <param name="id">The ID of the config object.</param>
        /// <returns>The config object, or null if not found.</returns>
        public static IConfigObject GetById(Type type, string id)
        {
            var candidates = new List<IConfigObject>();

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = GetAllConfigObjects(type).FirstOrDefault(candidate => candidate.Id == id);

            return result;
        }

        /// <summary>
        /// Load all library assets.
        /// </summary>
        public static void LoadAll()
        {
            LibraryParseErrors.Clear();

            // Load these in order of dependency.
            LoadOperations();
            LoadGoals();
            LoadArcs();

            ValidateNamedObjectCollection(LibraryGoals);
            ValidateNamedObjectCollection(LibraryOperations);
        }

        private static IEnumerable<IConfigObject> GetAllConfigObjects(Type type = null)
        {
            var objects = new IConfigObject[0].Concat(LibraryArcs).Concat(LibraryGoals).Concat(LibraryOperations);

            if (type != null)
            {
                objects = objects.Where(x => type.IsAssignableFrom(x.GetType()));
            }

            return objects;
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
                if (arc == null)
                {
                    Autoccultist.LogWarn($"Arc at {filePath} returned a null object.");
                    return;
                }

                LibraryArcs.Add(arc);
            }
            catch (YamlFileException ex)
            {
                LibraryParseErrors.Add(ex);
                Autoccultist.LogWarn(ex, $"Failed to load arc from file {filePath}");
            }
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
                if (goal == null)
                {
                    Autoccultist.LogWarn($"Goal at {filePath} returned a null object.");
                    return;
                }

                LibraryGoals.Add(goal);
            }
            catch (YamlFileException ex)
            {
                LibraryParseErrors.Add(ex);
                Autoccultist.LogWarn(ex, $"Failed to load goal from file {filePath}");
            }
        }

        private static void LoadOperations()
        {
            LibraryOperations.Clear();
            FilesystemHelpers.WalkDirectory(OperationsDirectory, LoadOperation);
        }

        private static void LoadOperation(string filePath)
        {
            try
            {
                var operation = Deserializer.Deserialize<OperationConfig>(filePath);
                if (operation == null)
                {
                    Autoccultist.LogWarn($"Operation at {filePath} returned a null object.");
                    return;
                }

                LibraryOperations.Add(operation);
            }
            catch (YamlFileException ex)
            {
                LibraryParseErrors.Add(ex);
                Autoccultist.LogWarn(ex, $"Failed to load operation from file {filePath}");
            }
        }

        private static void ValidateNamedObjectCollection(IReadOnlyCollection<NamedConfigObject> items)
        {
            var seenNames = new Dictionary<string, string>();
            foreach (var item in items)
            {
                if (item.AutoName)
                {
                    Autoccultist.LogWarn($"{item.GetType().Name} at {item.FilePath} was not given a name.");
                    continue;
                }

                if (seenNames.TryGetValue(item.Name.ToLower(), out var oldPath))
                {
                    Autoccultist.LogWarn($"{item.GetType().Name} named {item.Name} at {item.FilePath} has a name that conflicts with the item at {oldPath}.");
                }
                else
                {
                    seenNames.Add(item.Name.ToLower(), item.FilePath);
                }
            }
        }
    }
}
