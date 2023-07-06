namespace AutoccultistNS.Yaml
{
    using System;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.Utilities;

    public class WrappedValueDeserializer : IValueDeserializer
    {
        private readonly IValueDeserializer innerDeserializer;

        public WrappedValueDeserializer(IValueDeserializer innerDeserializer)
        {
            this.innerDeserializer = innerDeserializer;
        }

        public object DeserializeValue(IParser parser, Type expectedType, SerializerState state, IValueDeserializer nestedObjectDeserializer)
        {
            var start = parser.Current?.Start;
            try
            {
                return this.innerDeserializer.DeserializeValue(parser, expectedType, state, nestedObjectDeserializer);
            }
            catch (YamlFileException ex)
            {
                throw ex;
            }
            catch (YamlException ex)
            {
                // Record extra information on our failure.
                throw new YamlFileException(
                    Deserializer.CurrentFilePath,
                    ex.Start,
                    ex.End,
                    ex.Message,
                    ex);
            }
            catch (Exception ex)
            {
                // Record extra information on our failure.
                throw new YamlFileException(
                    Deserializer.CurrentFilePath,
                    start,
                    parser.Current?.Start ?? start,
                    $"While trying to deserialize {expectedType.Name}: {ex.Message}",
                    ex);
            }
        }
    }
}
