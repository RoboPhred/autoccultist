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
        public static bool CanExecute(this IImpulse impulse, IGameState state)
        {
            // Optionally check required cards for starting the impulse
            if (impulse.Requirements?.IsConditionMet(state) == false)
            {
                return false;
            }

            // Sometimes, we want to stop an impulse if other cards are present.
            if (impulse.Forbidders?.IsConditionMet(state) == true)
            {
                return false;
            }

            if (!impulse.Operation.IsConditionMet(state))
            {
                return false;
            }

            return true;
        }
    }
}
