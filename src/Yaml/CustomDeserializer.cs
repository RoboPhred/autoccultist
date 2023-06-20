namespace AutoccultistNS.Yaml
{
    using System;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// A node deserializer capable of deserializing multiple types base don duck type property matching.
    /// </summary>
    internal class CustomDeserializer : INodeDeserializer
    {
        /// <inheritdoc/>
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            var deserializer = CustomDeserializerAttribute.GetDeserializer(expectedType);
            if (deserializer == null)
            {
                value = null;
                return false;
            }

            return deserializer.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
        }
    }
}
