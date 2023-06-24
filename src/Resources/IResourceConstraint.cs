namespace AutoccultistNS.Resources
{
    using System;
    using System.Collections.Generic;

    public interface IResourceConstraint<out T> : IDisposable
        where T : class
    {
        event EventHandler Disposed;

        IEnumerable<T> GetCandidates();
    }
}
