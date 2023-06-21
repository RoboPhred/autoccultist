namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using YamlDotNet.Core;

    public class RememberConfig : NamedConfigObject, IReactor
    {
        /// <summary>
        /// Gets or sets the id of this memory.
        /// </summary>
        public string SetMemory { get; set; }

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
        public int? Set { get; set; }

        /// <summary>
        /// Add a value to the memory.
        /// </summary>
        public int? Increment { get; set; }

        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (string.IsNullOrEmpty(this.SetMemory))
            {
                throw new YamlException(start, end, "Remember must specify a memory.");
            }

            if (!this.SetMemory.All(c => char.IsLetterOrDigit(c) || c == '_'))
            {
                throw new YamlException(start, end, "Memory must only contain letters, numbers, and underscores.");
            }
        }

        public IReaction GetReaction()
        {
            if (this.Increment.HasValue)
            {
                return new MemoryReaction(this.SetMemory, this.Label, this.Description, this.Increment.Value, true);
            }
            else if (this.Set.HasValue)
            {
                return new MemoryReaction(this.SetMemory, this.Label, this.Description, this.Set.Value, false);
            }
            else
            {
                return new MemoryReaction(this.SetMemory, this.Label, this.Description, 1, false);
            }
        }
    }
}
