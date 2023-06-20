namespace AutoccultistNS.Yaml
{
    /// <summary>
    /// A wrapper for a value that should be cached in its unwrapped form.
    /// </summary>
    // This is beyond frustrating.  Having to use types as interpretations on parsing instead of simply decorating the property means
    // our reinterpretation types are getting cached as the values we want to store.
    public interface IYamlValueWrapper
    {
        /// <summary>
        /// Unwraps the value to a value safe for storing in the cache.
        /// </summary>
        object Unwrap();
    }
}
