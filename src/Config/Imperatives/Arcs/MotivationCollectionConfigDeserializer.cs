namespace AutoccultistNS.Config
{
    using System;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    public class MotivationCollectionConfigDeserializer : INodeDeserializer
    {
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (expectedType != typeof(MotivationCollectionConfig))
            {
                value = null;
                return false;
            }

            if (reader.Accept<SequenceStart>(out var _))
            {
                value = nestedObjectDeserializer(reader, typeof(LinearMotivationCollectionConfig));
                return true;
            }
            else if (reader.Accept<MappingStart>(out var _))
            {
                value = nestedObjectDeserializer(reader, typeof(ParallelMotivationCollectionConfig));
                return true;
            }
            else
            {
                throw new SemanticErrorException(reader.Current.Start, reader.Current.End, "Expected a sequence or mapping.");
            }
        }
    }
}
