namespace Autoccultist.Actor.Actions
{
    /// <summary>
    /// An action to set the paused state of the game.
    /// </summary>
    public class SetPausedAction : IAutoccultistAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetPausedAction"/> class.
        /// </summary>
        /// <param name="paused">The paused state to set.</param>
        public SetPausedAction(bool paused)
        {
            this.Paused = paused;
        }

        /// <summary>
        /// Gets a value indicating whether the action will pause or unpause the game.
        /// </summary>
        public bool Paused { get; }

        /// <inheritdoc/>
        public void Execute()
        {
            if (!GameAPI.IsInteractable)
            {
                throw new ActionFailureException(this, "Game is not interactable at this moment.");
            }

            GameAPI.SetPaused(this.Paused);
        }
    }
}
