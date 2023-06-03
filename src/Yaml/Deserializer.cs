namespace AutoccultistNS.Yaml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.NodeDeserializers;

    /// <summary>
    /// Deserialization utilities for yaml files.
    /// </summary>
    public static class Deserializer
    {
        /// <summary>
        /// Gets the naming convention used by this deserializer.
        /// </summary>
        public static readonly INamingConvention NamingConvention = CamelCaseNamingConvention.Instance;

        private static readonly Stack<string> ParsingFiles = new();

        private static readonly Dictionary<string, object> DeserializedObjectCache = new();

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
        /// Clears the cache of deserialized objects.
        /// </summary>
        public static void ClearCache()
        {
            DeserializedObjectCache.Clear();
        }

        /// <summary>
        /// Deserializes the given file into the given type.
        /// </summary>
        /// <param name="filePath">The path to the file to deserialize.</param>
        /// <param name="cache">Whether to cache the deserialized object.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>(string filePath, bool cache = true)
        {
            return DeserializeFromParser(
                filePath,
                parser =>
                {
                    var deserializer = BuildDeserializer();
                    return deserializer.Deserialize<T>(parser);
                },
                cache);
        }

        /// <summary>
        /// Deserialize the given file into the given type.
        /// </summary>
        /// <param name="filePath">The path to the file to deserialize.</param>
        /// <param name="type">The type to deserialize.</param>
        /// <param name="cache">Whether to cache the deserialized object.</param>
        /// <returns>The deserialized object.</returns>
        public static object Deserialize(string filePath, Type type, bool cache = true)
        {
            return DeserializeFromParser(
                filePath,
                parser =>
                {
                    var deserializer = BuildDeserializer();
                    return deserializer.Deserialize(parser, type);
                },
                cache);
        }

        /// <summary>
        /// Obtains a <see cref="IParser"/> from a given yaml file path.
        /// </summary>
        /// <param name="filePath">The path of the file to obtain a parser for.</param>
        /// <param name="func">The function to parse the object from the parser.</param>
        /// <param name="cache">Whether to cache the deserialized object.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>The parser for the given file.</returns>
        public static T DeserializeFromParser<T>(string filePath, Func<IParser, T> func, bool cache = true)
        {
            if (ParsingFiles.Contains(filePath))
            {
                throw new InvalidOperationException($"Circular import detected: {string.Join(" -> ", ParsingFiles.Reverse())} -> {filePath}");
            }

            if (cache && DeserializedObjectCache.TryGetValue(filePath, out var cached))
            {
                return (T)cached;
            }

            ParsingFiles.Push(filePath);
            try
            {
                var fileContents = File.ReadAllText(filePath);
                var parser = new MergingParser(new Parser(new StringReader(fileContents)));
                var result = func(parser);

                if (cache)
                {
                    DeserializedObjectCache[filePath] = result;
                }

                return result;
            }
            catch (YamlException ex) when (!(ex is YamlFileException))
            {
                NoonUtility.LogWarning(ex, $"Error parsing file {CurrentFilePath}: {ex.GetInnermostMessage()}");
                throw new YamlFileException(CurrentFilePath, ex.Start, ex.End, ex.GetInnermostMessage(), ex);
            }
            finally
            {
                ParsingFiles.Pop();
            }
        }

        private static IDeserializer BuildDeserializer()
        {
            return new DeserializerBuilder()
                    .WithNamingConvention(NamingConvention)
                    .WithNodeTypeResolver(new ImportNodeTypeResolver(), s => s.OnTop())
                    .WithNodeDeserializer(new ImportDeserializer(), s => s.OnTop())
                    .WithNodeDeserializer(new DuckTypeDeserializer(), s => s.OnTop())
                    .WithNodeDeserializer(objectDeserializer => new NodeDeserializer(objectDeserializer), s => s.InsteadOf<ObjectNodeDeserializer>())
                    .Build();
        }
    }
}
