namespace Autoccultist.GameState.Impl
{
    /// <summary>
    /// A base class for implementing game state objects.
    /// </summary>
    internal abstract class GameStateObject
    {
        private readonly long stateVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameStateObject"/> class.
        /// </summary>
        protected GameStateObject()
        {
            this.stateVersion = CurrentStateVersion;
        }

        /// <summary>
        /// Gets or sets the current extant state version.
        /// </summary>
        public static long CurrentStateVersion { get; set; } = 0;

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
    }
}
