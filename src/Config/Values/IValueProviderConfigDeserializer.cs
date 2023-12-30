namespace AutoccultistNS.Config.Values
{
    using System;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    public class IValueProviderConfigDeserializer : INodeDeserializer
    {
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (reader.TryConsume<Scalar>(out var scalar))
            {
                if (!int.TryParse(scalar.Value, out var intValue))
                {
                    throw new YamlException(scalar.Start, scalar.End, "ValueProvider must be an object or an integer value.");
                }

                value = new StaticValueProviderConfig(intValue);
                return true;
            }

            value = nestedObjectDeserializer(reader, typeof(ValueProviderObjectConfig)) as ValueProviderObjectConfig;
            return true;
        }
    }
}
