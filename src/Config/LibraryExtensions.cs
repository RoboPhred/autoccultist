namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;

    public static class LibraryExtensions
    {
        public static string GetLibraryPath(this IGoal goal)
        {
            var path = goal.FilePath;
            if (path.StartsWith(Library.GoalsDirectory))
            {
                return path.Substring(Library.GoalsDirectory.Length + 1);
            }

            return null;
        }

        public static string GetLibraryPath(this IArc arc)
        {
            var path = arc.FilePath;
            if (path.StartsWith(Library.ArcsDirectory))
            {
                return path.Substring(Library.ArcsDirectory.Length + 1);
            }

            return null;
        }
    }
}
