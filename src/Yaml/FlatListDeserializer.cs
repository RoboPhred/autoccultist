namespace AutoccultistNS.Yaml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Deserializes <see cref="FlatList"/> objects.
    /// </summary>
    public class FlatListDeserializer : INodeDeserializer
    {
        /// <inheritdoc/>
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (!expectedType.IsGenericType || expectedType.GetGenericTypeDefinition() != typeof(FlatList<>))
            {
                value = null;
                return false;
            }

            var subjectType = expectedType.GetGenericArguments()[0];

            NoonUtility.LogWarning($"Deserializing FlatList of type " + subjectType.Name);

            // We need to handle this specially, as we need to tunnel our "FlatList" into the inner document.
            if (ImportDeserializer.TryConsumeImport(reader, out var filePath))
            {
                NoonUtility.LogWarning($"...We are import");

                value = Deserializer.DeserializeFromParser(filePath, importParser =>
                {
                    // Expecting a basic, non fragment single document file.
                    importParser.Consume<StreamStart>();
                    importParser.Consume<DocumentStart>();

                    var innerValue = this.DeserializeFlatList(importParser, expectedType, subjectType, nestedObjectDeserializer);

                    importParser.Consume<DocumentEnd>();
                    importParser.Consume<StreamEnd>();

                    return innerValue;
                });

                return true;
            }
            else
            {
                NoonUtility.LogWarning($"...We are not import");

                value = this.DeserializeFlatList(reader, expectedType, subjectType, nestedObjectDeserializer);
                return true;
            }
        }

        private object DeserializeFlatList(IParser reader, Type expectedType, Type subjectType, Func<IParser, Type, object> nestedObjectDeserializer)
        {
            var items = new List<object>();
            reader.Consume<SequenceStart>();
            while (!reader.TryConsume<SequenceEnd>(out var _))
            {
                items.AddRange(this.DeserializeFlatListElement(reader, subjectType, nestedObjectDeserializer));
            }

            var itemsEnumerable = typeof(System.Linq.Enumerable).GetMethod(nameof(System.Linq.Enumerable.Cast)).MakeGenericMethod(subjectType).Invoke(null, new[] { items });
            var result = Activator.CreateInstance(expectedType, new[] { itemsEnumerable });

            return result;
        }

        private IEnumerable<object> DeserializeFlatListElement(IParser reader, Type subjectType, Func<IParser, Type, object> nestedObjectDeserializer)
        {
            NoonUtility.LogWarning($"...Got item in flat list");

            // TODO: Might be able to simplify this, especially the !import bit, by trying to call
            // nestedObjectDeserializer with the right target type (FlatList<T>), but it will have to accept a top-level Object instead of always being a list.

            var result = new List<object>();
            if (reader.TryConsume<SequenceStart>(out var _))
            {
                NoonUtility.LogWarning($"...Item is sequence");

                while (!reader.TryConsume<SequenceEnd>(out var _))
                {
                    NoonUtility.LogWarning($"...Handling item in sequence " + reader.Peek<NodeEvent>().ToString());
                    result.AddRange(this.DeserializeFlatListElement(reader, subjectType, nestedObjectDeserializer));
                }

                NoonUtility.LogWarning($"...Ended sequence");
            }
            else if (ImportDeserializer.TryConsumeImport(reader, out var filePath))
            {
                NoonUtility.LogWarning($"...Item is import");

                // This import could give us a list of a single item
                // important: If this gives us a single item, we MUST return that single item back through the parser for
                // caching reasons, as we need to cache what's in the file, not what we want
                var value = Deserializer.DeserializeFromParser(filePath, importParser =>
                {
                    // Expecting a basic, non fragment single document file.
                    importParser.Consume<StreamStart>();
                    importParser.Consume<DocumentStart>();

                    object innerValue;
                    if (!importParser.Accept<SequenceStart>(out var _))
                    {
                        NoonUtility.LogWarning($"...Item import is single");
                        // We have a single item, so we need to return it back through the parser
                        innerValue = nestedObjectDeserializer(importParser, subjectType);
                    }
                    else
                    {
                        NoonUtility.LogWarning($"...Item import is list");
                        // It's a list, parse it according to our own rules.
                        // Note: We are assuming this list is meant for us, so our cached value will be flattened!
                        innerValue = this.DeserializeFlatListElement(importParser, subjectType, nestedObjectDeserializer);
                    }

                    importParser.Consume<DocumentEnd>();
                    importParser.Consume<StreamEnd>();

                    return innerValue;
                });

                if (value is IEnumerable<object> enumerable)
                {
                    result.AddRange(enumerable);
                }
                else
                {
                    result.Add(value);
                }
            }
            else
            {
                NoonUtility.LogWarning($"...Item is not sequence " + reader.Peek<NodeEvent>().ToString());
                result.Add(nestedObjectDeserializer(reader, subjectType));
            }

            NoonUtility.LogWarning($"...Handled item in flat list");

            return result;
        }
    }
}
