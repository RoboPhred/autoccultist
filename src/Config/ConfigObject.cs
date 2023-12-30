namespace AutoccultistNS.Config
{
    using System.IO;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    public abstract class ConfigObject : IConfigObject, IAfterYamlDeserialization
    {
        /// <inheritdoc/>
        public string Id { get; private set; }

        /// <inheritdoc/>
        [YamlIgnore]
        public string FilePath { get; private set; }

        /// <inheritdoc/>
        [YamlIgnore]
        public Mark? Start { get; private set; }

        /// <inheritdoc/>
        [YamlIgnore]
        public Mark? End { get; private set; }

        public override string ToString()
        {
            return $"{this.GetType().Name}(FilePath = {FilesystemHelpers.GetRelativePath(this.FilePath, Autoccultist.AssemblyDirectory)}:{this.Start?.Line})";
        }

        public virtual void AfterDeserialized(Mark start, Mark end)
        {
            this.FilePath = AutoccultistNS.Yaml.Deserializer.CurrentFilePath;
            this.Start = start;
            this.End = end;

            if (string.IsNullOrEmpty(this.Id))
            {
                var id = this.FilePath;
                var rootPath = LibraryConfigObjectAttribute.GetLibraryPath(this.GetType()) ?? Autoccultist.AssemblyDirectory;
                id = FilesystemHelpers.GetRelativePath(id, rootPath);
                id = Path.ChangeExtension(id, null);
                id = id.Replace('\\', '/');

                if (start.Line > 1)
                {
                    id = $"{id}:{start.Line}";
                }

                this.Id = id;
            }
            else
            {
                var byId = Library.GetById(this.GetType(), this.Id);
                if (byId != null)
                {
                    throw new YamlException(start, end, $"Duplicate id '{this.Id}' of {this.GetType().Name} found in {this.FilePath}:{start.Line} and {byId.FilePath}:{byId.Start?.Line}.");
                }
            }
        }
    }
}
