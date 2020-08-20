namespace Autoccultist.Config
{
    /// <summary>
    /// Defines a node in the config tree.
    /// </summary>
    public interface IConfigObject
    {
        /// <summary>
        /// Validate the configuration node's configuration is valid.
        /// <para>
        /// Throws <see cref="Autoccultist.Brain.Config.InvalidConfigException"/> when the node is invalid.
        /// </summary>
        void Validate();
    }
}
