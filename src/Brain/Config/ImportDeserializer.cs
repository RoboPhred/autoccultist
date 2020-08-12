using System;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Autoccultist.Brain.Config
{
    class ImportDeserializer : INodeDeserializer
    {
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (reader.Current is NodeEvent nodeEvent)
            {
                if (nodeEvent.Tag == "!import")
                {
                    var filePath = reader.Consume<Scalar>().Value;
                    filePath = Path.Combine(Path.GetDirectoryName(Deserializer.CurrentFilePath), filePath);
                    value = Deserializer.Deserialize(filePath, expectedType);
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}