using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    public class BrainConfig
    {
        // Unity refuses to work with YamlDotNet
        // public static BrainConfig Load(string filePath)
        // {
        //     var deserializer = new DeserializerBuilder()
        //         .WithNamingConvention(new CamelCaseNamingConvention())
        //         .Build();
        //     var fileContents = File.ReadAllText(filePath);
        //     return deserializer.Deserialize<BrainConfig>(fileContents);
        // }

        public List<Goal> Goals { get; set; }
    }
}