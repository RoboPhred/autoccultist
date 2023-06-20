namespace AutoccultistNS.Yaml
{
    using YamlDotNet.Serialization;

    public class WrappedDictionaryNodeDeserializer : AfterDeserializeDeserializer
    {
        public WrappedDictionaryNodeDeserializer(INodeDeserializer parent) : base(parent)
        {
        }
    }
}
