namespace Autoccultist.Config
{
    using YamlDotNet.Core;

    /// <summary>
    /// Generates names for unnamed <see cref="INamedConfigObject"/>.
    /// </summary>
    public static class NameGenerator
    {
        /// <summary>
        /// Generates a name for the object in the current file.
        /// </summary>
        /// <param name="filePath">The path to the file containing the object to name.</param>
        /// <param name="start">The start of the object to name.</param>
        /// <returns>A name for the object.</returns>
        public static string GenerateName(string filePath, Mark start)
        {
            var relFile = FilesystemHelpers.GetRelativePath(filePath, AutoccultistPlugin.AssemblyDirectory);
            if (start != null)
            {
                return $"{relFile}:{start.Line}";
            }
            else
            {
                return relFile;
            }
        }
    }
}
