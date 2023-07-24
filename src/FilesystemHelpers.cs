namespace AutoccultistNS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Helpers for filesystem operations.
    /// </summary>
    public static class FilesystemHelpers
    {
        /// <summary>
        /// Walk a directory and all child directories, triggering a function on each file path.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to walk.</param>
        /// <param name="handleFile">The action to take on encountering a file.</param>
        public static void WalkDirectory(string directoryPath, Action<string> handleFile, Func<string, bool> directoryFilter = null)
        {
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                if (Path.GetFileName(file).StartsWith("."))
                {
                    continue;
                }

                handleFile(file);
            }

            foreach (var directory in Directory.GetDirectories(directoryPath))
            {
                if (Path.GetFileName(directory).StartsWith("."))
                {
                    continue;
                }

                if (directoryFilter != null && !directoryFilter(directory))
                {
                    continue;
                }

                WalkDirectory(directory, handleFile);
            }
        }

        public static void DeleteDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            WalkDirectory(directoryPath, (path) => File.Delete(path));
            Directory.Delete(directoryPath);
        }

        public static List<string> GetDirectories(string directoryPath)
        {
            var items = from item in Directory.GetDirectories(directoryPath)
                        where !Path.GetFileName(item).StartsWith(".")
                        orderby item.ToLower()
                        select item;
            return items.ToList();
        }

        public static string GetDirectorySiblingPrevious(string directoryPath)
        {
            var items = GetDirectories(directoryPath);
            var index = items.IndexOf(directoryPath);
            if (index == -1)
            {
                return null;
            }

            return items[(index + items.Count - 1) % items.Count];
        }

        public static string GetDirectorySiblingNext(string directoryPath)
        {
            var items = GetDirectories(directoryPath);
            var index = items.IndexOf(directoryPath);
            if (index == -1)
            {
                return null;
            }

            return items[(index + 1) % items.Count];
        }

        // https://weblog.west-wind.com/posts/2010/Dec/20/Finding-a-Relative-Path-in-NET

        /// <summary>
        /// Returns a relative path string from a full path based on a base path
        /// provided.
        /// </summary>
        /// <param name="fullPath">The path to convert. Can be either a file or a directory.</param>
        /// <param name="basePath">The base path on which relative processing is based. Should be a directory.</param>
        /// <returns>
        /// String of the relative path.
        /// </returns>
        public static string GetRelativePath(string fullPath, string basePath)
        {
            // Require trailing backslash for path
            if (!basePath.EndsWith("\\"))
            {
                basePath += "\\";
            }

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert back to backward slashes
            return relativeUri.ToString().Replace("/", "\\");
        }
    }
}
