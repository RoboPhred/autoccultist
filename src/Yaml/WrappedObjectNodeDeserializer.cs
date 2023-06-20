namespace AutoccultistNS.Yaml
{
    using YamlDotNet.Serialization;

    public class WrappedObjectNodeDeserializer : AfterDeserializeDeserializer
    {
        public WrappedObjectNodeDeserializer(INodeDeserializer parent) : base(parent)
        {
        }
    }
}
