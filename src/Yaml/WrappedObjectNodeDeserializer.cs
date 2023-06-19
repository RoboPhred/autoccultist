namespace AutoccultistNS.Yaml
{
    using System;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Wraps a <see cref="WrappedObjectNodeDeserializer"/> with our own modifications.
    /// This allows parsed objects to handle post-deserialization logic that is aware of its file and location.
    /// </summary>
    public class WrappedObjectNodeDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer nodeDeserializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="WrappedObjectNodeDeserializer"/> class.
        /// </summary>
        /// <param name="nodeDeserializer">The ancestor deserializer to use.</param>
        public WrappedObjectNodeDeserializer(INodeDeserializer nodeDeserializer)
        {
            this.nodeDeserializer = nodeDeserializer;
        }

        /// <inheritdoc/>
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            Mark start = null;
            if (reader.Accept<ParsingEvent>(out var parsingEvent))
            {
                start = parsingEvent?.Start;
            }

            NoonUtility.LogWarning($"Deserializing type " + expectedType.Name + " at " + start?.ToString());
            if (this.nodeDeserializer.Deserialize(reader, expectedType, nestedObjectDeserializer, out value))
            {
                if (value is IAfterYamlDeserialization afterDeserialized)
                {
                    afterDeserialized.AfterDeserialized(start, reader.Current?.End);
                }

                return true;
            }

            return false;
        }
    }
}
