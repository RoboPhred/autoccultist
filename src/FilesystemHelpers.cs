namespace Autoccultist
{
    using System;
    using System.IO;

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
        public static void WalkDirectory(string directoryPath, Action<string> handleFile)
        {
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                handleFile(file);
            }

            foreach (var directory in Directory.GetDirectories(directoryPath))
            {
                WalkDirectory(directory, handleFile);
            }
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
