namespace AutoccultistNS.Brain
{
    using AutoccultistNS.GameState;

    /// <summary>
    /// Extension methods for impulses.
    /// </summary>
    public static class ImpulseExtensions
    {
        /// <summary>
        /// Determines whether the impulse can execute given the supplied game state.
        /// </summary>
        /// <param name="impulse">The impulse to check.</param>
        /// <param name="state">The game state to check conditions against.</param>
        /// <param name="failureReason">The reason why the impulse cannot execute, if any.</param>
        /// <returns>True if this impulse can execute right away, or False if it cannot.</returns>
        public static bool CanExecute(this IImpulse impulse, IGameState state, out ConditionFailure failureReason)
        {
            // Optionally check required cards for starting the impulse
            if (impulse.Requirements != null && impulse.Requirements.IsConditionMet(state, out failureReason) == false)
            {
                failureReason = new AddendedConditionFailure(failureReason, "Requirements not met.");
                return false;
            }

            // Sometimes, we want to stop an impulse if other cards are present.
            if (impulse.Forbidders != null && impulse.Forbidders.IsConditionMet(state, out failureReason) == true)
            {
                failureReason = new GeneralConditionFailure("Forbidders are present (and cannot show a negative reason).");
                return false;
            }

            if (!impulse.Operation.IsConditionMet(state, out failureReason))
            {
                failureReason = new AddendedConditionFailure(failureReason, "Operation not ready.");
                return false;
            }

            return true;
        }
    }
}
