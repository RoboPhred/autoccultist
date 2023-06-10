namespace AutoccultistNS.Actor.Actions
{
    using AutoccultistNS.GameState;

    /// <summary>
    /// An action to accepts the results of a mansus visit and resume the game.
    /// </summary>
    public class AcceptMansusResultsAction : ActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptMansusResultsAction"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to dump.</param>
        public AcceptMansusResultsAction()
        {
        }

        /// <inheritdoc/>
        protected override ActionResult OnExecute()
        {
            if (!GameAPI.EmptyMansusEgress())
            {
                throw new ActionFailureException(this, "Failed to empty the mansus.");
            }

            GameAPI.UserUnpause();

            GameStateProvider.Invalidate();

            return ActionResult.Completed;
        }
    }
}
