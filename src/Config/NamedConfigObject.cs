namespace AutoccultistNS.Config
{
    using YamlDotNet.Core;

    public abstract class NamedConfigObject : ConfigObject, INamedConfigObject
    {
        public string Name { get; set; }

        public bool AutoName { get; private set; } = false;

        public override string ToString()
        {
            return $"{this.GetType().Name}(Name = {this.Name}, FilePath = {FilesystemHelpers.GetRelativePath(this.FilePath, Autoccultist.AssemblyDirectory)}:{this.Start?.Line})";
        }

        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                this.AutoName = true;
                this.Name = NameGenerator.GenerateName(this.FilePath, start);
            }
        }
    }
}
