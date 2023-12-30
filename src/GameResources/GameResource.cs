namespace AutoccultistNS.GameResources
{
    using System;
    using System.Collections.Generic;

    public static class GameResource
    {
        private static readonly Dictionary<Type, object> Resources = new();

        public static GameResource<T> Of<T>()
            where T : class
        {
            if (!Resources.ContainsKey(typeof(T)))
            {
                Resources[typeof(T)] = new GameResource<T>();
            }

            return (GameResource<T>)Resources[typeof(T)];
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
