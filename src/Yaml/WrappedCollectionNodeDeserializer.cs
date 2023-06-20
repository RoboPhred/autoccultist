namespace AutoccultistNS.Yaml
{
    using YamlDotNet.Serialization;

    public class WrappedCollectionNodeDeserializer : AfterDeserializeDeserializer
    {
        public WrappedCollectionNodeDeserializer(INodeDeserializer parent)
            : base(parent)
        {
        }
    }
}
