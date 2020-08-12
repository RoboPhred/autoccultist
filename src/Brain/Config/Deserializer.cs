using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Autoccultist.Brain.Config
{
    static class Deserializer
    {
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
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .WithNodeTypeResolver(new ImportNodeTypeResolver(), s => s.OnTop())
                    .WithNodeDeserializer(new ImportDeserializer(), s => s.OnTop())
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