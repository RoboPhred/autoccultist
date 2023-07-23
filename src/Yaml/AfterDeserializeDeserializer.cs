namespace AutoccultistNS.Yaml
{
    using System;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Wraps a <see cref="INodeDeserializer"/> with logic to call <see cref="IAfterYamlDeserialization"/>.
    /// </summary>
    public class AfterDeserializeDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer nodeDeserializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AfterDeserializeDeserializer"/> class.
        /// </summary>
        /// <param name="nodeDeserializer">The ancestor deserializer to use.</param>
        public AfterDeserializeDeserializer(INodeDeserializer nodeDeserializer)
        {
            this.nodeDeserializer = nodeDeserializer;
        }

        /// <inheritdoc/>
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            Mark start = Mark.Empty;
            if (reader.Accept<ParsingEvent>(out var parsingEvent))
            {
                start = parsingEvent.Start;
            }

            if (this.nodeDeserializer.Deserialize(reader, expectedType, nestedObjectDeserializer, out value))
            {
                if (value is IAfterYamlDeserialization afterDeserialized)
                {
                    afterDeserialized.AfterDeserialized(start, reader.Current?.End ?? Mark.Empty);
                }

                return true;
            }

            return false;
        }
    }
}
