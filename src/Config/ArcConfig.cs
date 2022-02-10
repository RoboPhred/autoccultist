namespace Autoccultist.Config
{
    using System.Collections.Generic;
    using Autoccultist.Brain;
    using Autoccultist.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// The configuration for an <see cref="IArc"/>.
    /// </summary>
    public class ArcConfig : IConfigObject, IArc, IAfterYamlDeserialization
    {
        /// <summary>
        /// Gets or sets a list of motivations that will drive the execution of this arc.
        /// </summary>
        public List<MotivationConfig> Motivations { get; set; } = new();

        /// <inheritdoc/>
        IReadOnlyList<IMotivation> IArc.Motivations => this.Motivations;

        /// <summary>
        /// Loads an ArcConfig from the given file.
        /// </summary>
        /// <param name="filePath">The path to the config file to load.</param>
        /// <returns>A ArcConfig loaded from the file.</returns>
        /// <exception cref="Brain.Config.InvalidConfigException">The config file at the path contains invalid configuration.</exception>
        public static ArcConfig Load(string filePath)
        {
            return Deserializer.Deserialize<ArcConfig>(filePath);
        }

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (this.Motivations == null || this.Motivations.Count == 0)
            {
                throw new InvalidConfigException("Brain must have at least one motivation.");
            }
        }
    }
}
