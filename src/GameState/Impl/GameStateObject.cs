namespace AutoccultistNS.GameState.Impl
{
    using System;

    /// <summary>
    /// A base class for implementing game state objects.
    /// </summary>
    internal abstract class GameStateObject : IContentHashable
    {
        private readonly long stateVersion;

        private readonly Lazy<int> contentHash;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameStateObject"/> class.
        /// </summary>
        protected GameStateObject()
        {
            this.stateVersion = CurrentStateVersion;
            this.contentHash = new Lazy<int>(this.ComputeContentHash);
        }

        /// <summary>
        /// Gets or sets the current extant state version.
        /// </summary>
        public static long CurrentStateVersion { get; set; } = 0;

        public int GetContentHash()
        {
            this.VerifyAccess();
            return this.contentHash.Value;
        }

        /// <summary>
        /// Verify that this state object can be accessed.
        /// </summary>
        protected void VerifyAccess()
        {
            if (this.stateVersion != CurrentStateVersion)
            {
                throw new OutdatedStateException();
            }
        }

        protected abstract int ComputeContentHash();
    }
}
