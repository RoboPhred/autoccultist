namespace AutoccultistNS.Config
{
    using System.Linq;
    using AutoccultistNS.Brain;
    using YamlDotNet.Core;

    public class RememberReactorConfig : NamedConfigObject, IReactorConfig
    {
        /// <summary>
        /// Gets or sets the id of this memory.
        /// </summary>
        public string Remember { get; set; }

        /// <summary>
        /// Gets or sets the label of the memory.
        /// If not specified, the memory's label will not be modified.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the description of the memory.
        /// If not specified, the memory's description will not be modified.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Set the memory value to a specific value.
        /// </summary>
        public int? Value { get; set; }

        /// <summary>
        /// Adds the value to the memory.
        /// </summary>
        public bool Increment { get; set; } = false;

        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (string.IsNullOrEmpty(this.Remember))
            {
                throw new YamlException(start, end, "Remember must specify a memory.");
            }

            if (!this.Remember.All(c => char.IsLetterOrDigit(c) || c == '_'))
            {
                throw new YamlException(start, end, "Memory must only contain letters, numbers, and underscores.");
            }

            Hippocampus.RegisterMemory(this.Remember, this.Label, this.Description);
        }

        public IReaction GetReaction()
        {
            return new MemoryReaction(this.Remember, this.Label, this.Description, this.Value ?? 1, this.Increment);
        }
    }
}
