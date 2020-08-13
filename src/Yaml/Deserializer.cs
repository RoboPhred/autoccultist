using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Autoccultist.Yaml
{
    static class Deserializer
    {
        public static readonly INamingConvention NamingConvention = CamelCaseNamingConvention.Instance;
        private static Stack<string> parsingFiles = new Stack<string>();

        public static string CurrentFilePath
        {
            get
            {
                if (parsingFiles.Count == 0)
                {
                    return null;
                }

                return parsingFiles.Peek();
            }
        }
        public static T Deserialize<T>(string filePath)
        {
            parsingFiles.Push(filePath);
            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(NamingConvention)
                    // TODO: Remove this.  Currently needed because of merge keys.
                    // We really do not want to ignore them, and instead should give the user an error.
                    //  This is paritcularly useful for the duck typing mechanism.
                    .IgnoreUnmatchedProperties()
                    .WithNodeTypeResolver(new ImportNodeTypeResolver(), s => s.OnTop())
                    .WithNodeDeserializer(new ImportDeserializer(), s => s.OnTop())
                    .WithNodeDeserializer(new DuckTypeDeserializer(), s => s.OnBottom())
                    .Build();
                var fileContents = File.ReadAllText(filePath);
                var parser = new MergingParser(new Parser(new StringReader(fileContents)));
                return deserializer.Deserialize<T>(parser);
            }
            finally
            {
                parsingFiles.Pop();
            }
        }

        public static object Deserialize(string filePath, Type type)
        {
            parsingFiles.Push(filePath);
            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                var fileContents = File.ReadAllText(filePath);
                var parser = new MergingParser(new Parser(new StringReader(fileContents)));
                return deserializer.Deserialize(parser, type);
            }
            finally
            {
                parsingFiles.Pop();
            }
        }
    }
}