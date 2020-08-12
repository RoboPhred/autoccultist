using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Autoccultist.Brain.Config
{
    public class BrainConfig
    {
        public static BrainConfig Load(string filePath)
        {
            var config = Deserializer.Deserialize<BrainConfig>(filePath);
            var serializer = new SerializerBuilder()
                .Build();

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