namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CompoundReaction : IReaction
    {
        private readonly HashSet<IReaction> incomplete = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundReaction"/> class.
        /// </summary>
        /// <param name="reactions">The list of reactions that make up this compound reaction.</param>
        public CompoundReaction(IReadOnlyCollection<IReaction> reactions)
        {
            if (reactions.Count == 0)
            {
                throw new ArgumentException("Cannot create a CompoundReaction with no reactions.");
            }

            this.Reactions = reactions;
        }

        /// <inheritdoc/>
        public event EventHandler Completed;

        /// <summary>
        /// Gets the list of reactions that make up this compound reaction.
        /// </summary>
        public IReadOnlyCollection<IReaction> Reactions { get; }

        /// <inheritdoc/>
        public void Abort()
        {
            foreach (var reaction in this.Reactions)
            {
                reaction.Abort();
            }
        }

        /// <inheritdoc/>
        public void Start()
        {
            foreach (var reaction in this.Reactions)
            {
                this.incomplete.Add(reaction);
                reaction.Completed += this.OnReactionComplete;
                reaction.Start();
            }
        }

        public override string ToString()
        {
            var result = this.Reactions.First().ToString();
            if (this.Reactions.Count > 1)
            {
                result += $" (plus {this.Reactions.Count - 1} more)";
            }

            return result;
        }

        private void OnReactionComplete(object sender, EventArgs e)
        {
            var reaction = (IReaction)sender;
            reaction.Completed -= this.OnReactionComplete;

            if (!this.incomplete.Remove(reaction))
            {
                return;
            }

            if (this.incomplete.Count == 0)
            {
                this.Completed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
