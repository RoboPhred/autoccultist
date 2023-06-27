namespace AutoccultistNS.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Resources;

    public abstract class SituationReaction : IReaction, IResourceConstraint<ISituationState>
    {
        private bool isDisposed = false;
        private bool isCompleted = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SituationReaction"/> class.
        /// </summary>
        /// <param name="situationId">The ID of the situation this reaction is for.</param>
        protected SituationReaction(string situationId)
        {
            this.SituationId = situationId;
        }

        /// <inheritdoc/>
        public event EventHandler Completed;

        /// <inheritdoc/>
        public event EventHandler Disposed;

        /// <summary>
        /// Gets the ID of the situation this reaction is for.
        /// </summary>
        public string SituationId { get; private set; }

        /// <inheritdoc/>
        IEnumerable<ISituationState> IResourceConstraint<ISituationState>.GetCandidates()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public abstract void Start();

        public void Abort()
        {
            this.Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            this.OnAbort();
        }

        /// <inheritdoc/>
        protected abstract void OnAbort();

        /// <summary>
        /// Tries to mark this reaction as completed.
        /// If the reaction is already completed, nothing will happen.
        /// </summary>
        protected void TryComplete()
        {
            if (this.isCompleted)
            {
                return;
            }

            this.isCompleted = true;

            this.Completed?.Invoke(this, EventArgs.Empty);
            this.Disposed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the state of the situation this reaction is for.
        /// </summary>
        protected ISituationState GetSituationState()
        {
            var state = GameStateProvider.Current.Situations.FirstOrDefault(x => x.SituationId == this.SituationId);
            if (state == null)
            {
                throw new ReactionFailedException($"Situation {this.SituationId} does not exist.");
            }

            return state;
        }
    }
}
