namespace AutoccultistNS.Config
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class LibraryPathAttribute : Attribute
    {
        public LibraryPathAttribute(string path)
        {
            this.Path = path;
        }

        public string Path { get; }

        public static string GetLibraryPath(Type type)
        {
            var attribute = (LibraryPathAttribute)Attribute.GetCustomAttribute(type, typeof(LibraryPathAttribute));
            return attribute?.Path;
        }
    }
}