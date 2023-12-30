namespace AutoccultistNS.Brain
{
    using System;

    public class MemoryReaction : IReaction
    {
        private readonly string id;
        private readonly string label;
        private readonly string description;
        private readonly int value;
        private readonly bool increment;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryReaction"/> class.
        /// </summary>
        /// <param name="id">The id of the memory to set.</param>
        /// <param name="label">The label of the memory to set.</param>
        /// <param name="description">The description of the memory to set.</param>
        /// <param name="value">The value to set the memory to.</param>
        /// <param name="increment">Whether to increment the memory instead of setting it.</param>
        public MemoryReaction(string id, string label, string description, int value, bool increment)
        {
            this.id = id;
            this.label = label;
            this.description = description;
            this.value = value;
            this.increment = increment;
        }

        /// <inheritdoc/>
        public event EventHandler<ReactionEndedEventArgs> Ended;

        /// <inheritdoc/>
        public void Abort()
        {
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (this.increment)
            {
                Hippocampus.AddMemory(this.id, this.label, this.description, this.value);
            }
            else
            {
                Hippocampus.SetMemory(this.id, this.label, this.description, this.value);
            }

            this.Ended?.Invoke(this, new ReactionEndedEventArgs(false));
        }
    }
}
