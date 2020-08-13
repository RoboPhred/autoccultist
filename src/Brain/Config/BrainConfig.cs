using System.Collections.Generic;
using Autoccultist.Yaml;

namespace Autoccultist.Brain.Config
{
    public class BrainConfig
    {
        public static BrainConfig Load(string filePath)
        {
            var config = Deserializer.Deserialize<BrainConfig>(filePath);
            config.Validate();

            return config;
        }

        // Unused; here because YamlDotNet ignores anchors on unparsed properties
        public List<Imperative> SharedImperatives { get; set; }

        public List<Goal> Goals { get; set; }

        public void Validate()
        {
            if (this.Goals == null || this.Goals.Count == 0)
            {
                throw new InvalidConfigException("Brain must have at least one goal.");
            }
            foreach (var goal in this.Goals)
            {
                goal.Validate();
            }
        }
    }
}