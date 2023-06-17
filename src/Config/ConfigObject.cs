namespace AutoccultistNS.Config
{
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    public abstract class ConfigObject : IConfigObject, IAfterYamlDeserialization
    {
        /// <inheritdoc/>
        public string FilePath { get; private set; }

        /// <inheritdoc/>
        public Mark Start { get; private set; }

        /// <inheritdoc/>
        public Mark End { get; private set; }

        public virtual void AfterDeserialized(Mark start, Mark end)
        {
            this.FilePath = Deserializer.CurrentFilePath;
            this.Start = start;
            this.End = end;
        }
    }
}