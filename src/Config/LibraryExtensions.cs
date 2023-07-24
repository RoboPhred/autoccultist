namespace AutoccultistNS.Config
{
    using System.IO;
    using AutoccultistNS.Brain;

    public static class LibraryExtensions
    {
        public static string GetLibraryPath(this IImperative imperative)
        {
            if (imperative is not IConfigObject configObject)
            {
                return null;
            }

            var libraryPath = LibraryConfigObjectAttribute.GetLibraryPath(imperative.GetType());
            if (libraryPath == null)
            {
                NoonUtility.LogWarning("Unable to determine library path for " + imperative.GetType().FullName);
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
