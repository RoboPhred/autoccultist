namespace Autoccultist.Config
{
    using System.Collections.Generic;
    using Autoccultist.Brain;
    using Autoccultist.Config.Conditions;
    using Autoccultist.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// The configuration for an <see cref="IArc"/>.
    /// </summary>
    public class ArcConfig : IConfigObject, IArc, IAfterYamlDeserialization
    {
        /// <summary>
        /// Gets or sets the name of this arc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a list of motivations that will drive the execution of this arc.
        /// </summary>
        public List<MotivationConfig> Motivations { get; set; } = new();

        /// <summary>
        /// Gets or sets the selection hint to be used to determine the current arc on loading a save.
        /// </summary>
        public IGameStateConditionConfig SelectionHint { get; set; }

        /// <inheritdoc/>
        IReadOnlyList<IMotivation> IArc.Motivations => this.Motivations;

        /// <inheritdoc/>
        IGameStateCondition IArc.SelectionHint => this.SelectionHint;

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
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            if (this.Motivations == null || this.Motivations.Count == 0)
            {
                throw new InvalidConfigException("Brain must have at least one motivation.");
            }
        }
    }
}
