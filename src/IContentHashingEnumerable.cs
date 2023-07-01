namespace AutoccultistNS
{
    /// <summary>
    /// Indicates an enumerable that returns a hash code reflective of its contents.
    /// </summary>
    public interface IContentHashingEnumerable
    {
        int GetUnorderedContentHash();
    }
}
