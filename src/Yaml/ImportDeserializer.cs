namespace Autoccultist.Yaml
{
    using System;
    using System.IO;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// A deserializer capable of importing additional yaml files based on `!import` statements.
    /// </summary>
    internal class ImportDeserializer : INodeDeserializer
    {
        /// <summary>
        /// Try to consume an import tag node.
        /// </summary>
        /// <param name="reader">The parser to consume from.</param>
        /// <param name="filePath">The resulting file path of the import node, if any.</param>
        /// <returns>True if an import node was consumed, otherwise False.</returns>
        public static bool TryConsumeImport(IParser reader, out string filePath)
        {
            if (reader.Accept<NodeEvent>(out var nodeEvent) && nodeEvent.Tag == "!import")
            {
                reader.Consume<NodeEvent>();
                filePath = reader.Consume<Scalar>().Value;
                filePath = Path.Combine(Path.GetDirectoryName(Deserializer.CurrentFilePath), filePath);
                return true;
            }

            filePath = null;
            return false;
        }

        /// <inheritdoc/>
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (reader.Current is NodeEvent nodeEvent && nodeEvent.Tag == "!import")
            {
                var filePath = reader.Consume<Scalar>().Value;
                filePath = Path.Combine(Path.GetDirectoryName(Deserializer.CurrentFilePath), filePath);
                if (!File.Exists(filePath))
                {
                    throw new YamlException(nodeEvent.Start, nodeEvent.End, $"Cannot import file \"{filePath}\" as the file does not exist.");
                }

                value = Deserializer.Deserialize(filePath, expectedType);
                return true;
            }

            value = null;
            return false;
        }
    }
}
