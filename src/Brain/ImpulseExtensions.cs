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
        /// <returns>True if this impulse can execute right away, or False if it cannot.</returns>
        public static ConditionResult CanExecute(this IImpulse impulse, IGameState state)
        {
            var impulseReady = impulse.IsConditionMet(state);
            if (!impulseReady)
            {
                return new AddendedConditionFailure(impulseReady, "Requirements not met.");
            }

            var operationReady = impulse.Operation.IsConditionMet(state);
            if (!operationReady)
            {
                return new AddendedConditionFailure(operationReady, "Operation not ready.");
            }

            return ConditionResult.Success;
        }
    }
}
