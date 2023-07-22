namespace AutoccultistNS.Config
{
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Defines a node in the config tree.
    /// </summary>
    public interface IConfigObject : IIdObject
    {
        /// <summary>
        /// Gets the path to the file that this object was loaded from.
        /// </summary>
        [YamlIgnore]
        string FilePath { get; }

        /// <summary>
        /// Gets the location of the start of this config object in the file.
        /// </summary>
        [YamlIgnore]
        Mark Start { get; }

        /// <summary>
        /// Gets the location of the end of this config object in the file.
        /// </summary>
        [YamlIgnore]
        Mark End { get; }
    }
}
