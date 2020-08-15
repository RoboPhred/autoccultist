namespace Autoccultist.Yaml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    /// <summary>
    /// Deserialization utilities for yaml files.
    /// </summary>
    public static class Deserializer
    {
        /// <summary>
        /// Gets the naming convention used by this deserializer.
        /// </summary>
        public static readonly INamingConvention NamingConvention = CamelCaseNamingConvention.Instance;

        private static readonly Stack<string> ParsingFiles = new Stack<string>();

        /// <summary>
        /// Gets the path of the current file being processed.
        /// If no file is being processed, returns null.
        /// </summary>
        public static string CurrentFilePath
        {
            get
            {
                if (ParsingFiles.Count == 0)
                {
                    return null;
                }

                return ParsingFiles.Peek();
            }
        }

        /// <summary>
        /// Deserializes the given file into the given type.
        /// </summary>
        /// <param name="filePath">The path to the file to deserialize.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>The deserialized type.</returns>
        public static T Deserialize<T>(string filePath)
        {
            ParsingFiles.Push(filePath);
            try
            {
                var deserializer = BuildDeserializer();
                var fileContents = File.ReadAllText(filePath);
                var parser = new MergingParser(new Parser(new StringReader(fileContents)));
                return deserializer.Deserialize<T>(parser);
            }
            finally
            {
                ParsingFiles.Pop();
            }
        }

        /// <summary>
        /// Deserialize the given file into the given type.
        /// </summary>
        /// <param name="filePath">The path to the file to deserialize.</param>
        /// <param name="type">The type to deserialize.</param>
        /// <returns>The deserialized type.</returns>
        public static object Deserialize(string filePath, Type type)
        {
            ParsingFiles.Push(filePath);
            try
            {
                var deserializer = BuildDeserializer();
                var fileContents = File.ReadAllText(filePath);
                var parser = new MergingParser(new Parser(new StringReader(fileContents)));
                return deserializer.Deserialize(parser, type);
            }
            finally
            {
                ParsingFiles.Pop();
            }
        }

        private static IDeserializer BuildDeserializer()
        {
            // TODO: We should remove IgnoreUnmatchedProperties, as it is useful to know if we made a typo.
            return new DeserializerBuilder()
                    .WithNamingConvention(NamingConvention)
                    .IgnoreUnmatchedProperties()
                    .WithNodeTypeResolver(new ImportNodeTypeResolver(), s => s.OnTop())
                    .WithNodeDeserializer(new ImportDeserializer(), s => s.OnTop())
                    .WithNodeDeserializer(new DuckTypeDeserializer(), s => s.OnTop())
                    .Build();
        }
    }
}
