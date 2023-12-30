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
            filePath = Path.GetFullPath(filePath);

            if (ParsingFiles.Contains(filePath))
            {
                throw new InvalidOperationException($"Circular import detected: {string.Join(" -> ", ParsingFiles.Reverse())} -> {filePath}");
            }

            if (cache && DeserializedObjectCache.TryGetValue(filePath, out var cached))
            {
                if (typeof(T).IsAssignableFrom(cached.GetType()))
                {
                    return (T)cached;
                }
                else
                {
                    // Our cached object isnt actually assignable to this.  That probably means we saw an !import targeting a base type
                    // and are now parsing it as derived.
                    // We should probably cache based on the resultant type, and support AssignableFrom checks.
                    // For now, lets just disable caching when weirdness like this happens.
                    cache = false;
                }
            }

            ParsingFiles.Push(filePath);
            try
            {
                var fileContents = File.ReadAllText(filePath);
                using (var textReader = new StringReader(fileContents))
                {
                    var parser = new MergingParser(new Parser(textReader));
                    var result = func(parser);

                    if (cache)
                    {
                        // FIXME: This is here because some types are !import-ed into different targets.
                        // However, this only handles the case where we get the more specific derived type imports first.
                        // We should instead cache based on resultant type, and support AssignableFrom checks.
                        // To keep the references the same, maybe we should also have base types able to annotate a ParseAsType marker to force
                        // the parser into the more derived type.
                        if (result is IYamlValueWrapper wrapper)
                        {
                            DeserializedObjectCache[filePath] = wrapper.Unwrap();
                        }
                        else
                        {
                            DeserializedObjectCache[filePath] = result;
                        }
                    }

                    return result;
                }
            }
            catch (YamlException ex) when (!(ex is YamlFileException))
            {
                Autoccultist.LogWarn(ex, $"Error parsing file {CurrentFilePath}: {ex.GetInnermostMessage()}");
                throw new YamlFileException(CurrentFilePath, ex.Start, ex.End, ex.GetInnermostMessage(), ex);
            }
            finally
            {
                ParsingFiles.Pop();
            }
        }

        private static IDeserializer BuildDeserializer()
        {
            // TODO: Use CustomDeserializer attribute instead of FlatListDeserializer.  Tried to do it but got errors.
            var valueDeserializer = new DeserializerBuilder()
                    .WithNamingConvention(NamingConvention)
                    .WithNodeTypeResolver(new ImportNodeTypeResolver(), s => s.OnTop())
                    .WithNodeDeserializer(new CustomDeserializer(), s => s.OnTop())
                    .WithNodeDeserializer(new FlatListDeserializer(), s => s.After<CustomDeserializer>())
                    .WithNodeDeserializer(new DuckTypeDeserializer(), s => s.After<FlatListDeserializer>())
                    .WithNodeDeserializer(new ImportDeserializer(), s => s.After<DuckTypeDeserializer>())
                    .WithNodeDeserializer(deserializer => new WrappedDictionaryNodeDeserializer(deserializer), s => s.InsteadOf<DictionaryNodeDeserializer>())
                    .WithNodeDeserializer(deserializer => new WrappedCollectionNodeDeserializer(deserializer), s => s.InsteadOf<CollectionNodeDeserializer>())
                    .WithNodeDeserializer(deserializer => new WrappedObjectNodeDeserializer(deserializer), s => s.InsteadOf<ObjectNodeDeserializer>())
                    .BuildValueDeserializer();
            return YamlDotNet.Serialization.Deserializer.FromValueDeserializer(new WrappedValueDeserializer(valueDeserializer));
        }
    }
}
