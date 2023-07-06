namespace AutoccultistNS
{
    /// <summary>
    /// Represents an object that can be hashed based on its content.
    /// </summary>
    public interface IContentHashable
    {
        /// <summary>
        /// Gets a hash of the content of this object.
        /// </summary>
        int GetContentHash();
    }
}
