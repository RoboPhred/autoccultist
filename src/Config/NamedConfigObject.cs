namespace AutoccultistNS.Config
{
    using YamlDotNet.Core;

    public abstract class NamedConfigObject : ConfigObject, INamedConfigObject
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{this.GetType().Name}(Name = {this.Name})";
        }

        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                this.Name = NameGenerator.GenerateName(this.FilePath, start);
            }
        }
    }
}
