namespace AutoccultistNS.Config
{
    using System.IO;
    using AutoccultistNS.Brain;

    public static class LibraryExtensions
    {
        public static string GetLibraryPath(this IImperative goal)
        {
            if (goal is not IConfigObject configObject)
            {
                return null;
            }

            var libraryPath = LibraryPathAttribute.GetLibraryPath(goal.GetType());
            if (libraryPath == null)
            {
                return null;
            }

            libraryPath = Path.Combine(Autoccultist.AssemblyDirectory, libraryPath);

            var path = configObject.FilePath;
            if (path.StartsWith(libraryPath))
            {
                return path.Substring(libraryPath.Length + 1);
            }

            return null;
        }
    }
}
