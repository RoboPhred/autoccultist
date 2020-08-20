namespace Autoccultist.Config
{
    /// <summary>
    /// Defines a configuration node with a human friendly display name.
    /// </summary>
    public interface INamedConfigObject : IConfigObject
    {
        /// <summary>
        /// Gets or sets the human-friendly display name of this goal.
        /// </summary>
        string Name { get; set; }
    }
}
