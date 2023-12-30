namespace AutoccultistNS.Config
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class LibraryConfigObjectAttribute : Attribute
    {
        public LibraryConfigObjectAttribute(string path)
        {
            this.Path = path;
        }

        public string Path { get; }

        public static string GetLibraryPath(Type type)
        {
            var attribute = (LibraryConfigObjectAttribute)Attribute.GetCustomAttribute(type, typeof(LibraryConfigObjectAttribute));
            if (string.IsNullOrEmpty(attribute?.Path))
            {
                return null;
            }

            return System.IO.Path.Combine(Autoccultist.AssemblyDirectory, attribute?.Path);
        }
    }
}
