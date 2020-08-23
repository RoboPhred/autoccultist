namespace Autoccultist.Config
{
    using System.Collections.Generic;
    using Autoccultist.Brain;
    using Autoccultist.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// A full configuration object representing instructions to play a game of Cultist Simulator.
    /// </summary>
    public class BrainConfig : IConfigObject, IBrain, IAfterYamlDeserialization
    {
        /// <summary>
        /// Gets or sets a list of goals for this brain config to achieve.
        /// <para>
        /// Goals will operate in order, skipping over goals until one is found that can be activated.
        /// </summary>
        public List<GoalConfig> Goals { get; set; }

        /// <inheritdoc/>
        IReadOnlyList<IGoal> IBrain.Goals => this.Goals;

        /// <summary>
        /// Loads a BrainConfig from the given file.
        /// </summary>
        /// <param name="filePath">The path to the config file to load.</param>
        /// <returns>A BrainConfig loaded from the file.</returns>
        /// <exception cref="Autoccultist.Brain.Config.InvalidConfigException">The config file at the path contains invalid configuration.</exception>
        public static BrainConfig Load(string filePath)
        {
            return Deserializer.Deserialize<BrainConfig>(filePath);
        }

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (this.Goals == null || this.Goals.Count == 0)
            {
                throw new InvalidConfigException("Brain must have at least one goal.");
            }
        }
    }
}
