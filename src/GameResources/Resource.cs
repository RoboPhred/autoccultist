namespace AutoccultistNS.GameResources
{
    using System;
    using System.Collections.Generic;

    public static class Resource
    {
        private static readonly Dictionary<Type, object> Resources = new();

        public static Resource<T> Of<T>()
            where T : class
        {
            if (!Resources.ContainsKey(typeof(T)))
            {
                Resources[typeof(T)] = new Resource<T>();
            }

            return (Resource<T>)Resources[typeof(T)];
        }

        public static void ClearAll()
        {
            foreach (var resource in Resources.Values)
            {
                if (resource is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            Resources.Clear();
        }
    }
}
