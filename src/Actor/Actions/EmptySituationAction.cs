namespace AutoccultistNS.Actor.Actions
{
    using AutoccultistNS.GameState;
    using SecretHistories.Enums;

    /// <summary>
    /// An action to dump all cards out of a situation window.
    /// Supports unstarted and completed situations.
    /// </summary>
    public class EmptySituationAction : ActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptySituationAction"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to dump.</param>
        public EmptySituationAction(string situationId)
        {
            this.SituationId = situationId;
        }

        /// <summary>
        /// Gets the situation id of the situation this action is targeting.
        /// </summary>
        public string SituationId { get; }

        /// <inheritdoc/>
        public override void Execute()
        {
            this.VerifyNotExecuted();

            if (GameAPI.IsInMansus)
            {
                throw new ActionFailureException(this, "Cannot interact with situations when in the mansus.");
            }

            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                throw new ActionFailureException(this, "Situation is not available.");
            }

            if (situation.State.Identifier == StateEnum.Unstarted)
            {
                situation.DumpUnstartedBusiness();
            }
            else if (situation.State.Identifier == StateEnum.Complete)
            {
                situation.Conclude();
            }
            else
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} cannot be emptied becuase it is in state {situation.State.Identifier}.");
            }

            GameStateProvider.Invalidate();
        }
    }
}