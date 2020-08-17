namespace Autoccultist.Brain
{
    using Autoccultist.GameState;

    /// <summary>
    /// Extension methods for imperatives.
    /// </summary>
    public static class ImperativeExtensions
    {
        /// <summary>
        /// Determines whether the imperative can execute given the supplied game state.
        /// </summary>
        /// <param name="imperative">The imperative to check.</param>
        /// <param name="state">The game state to check conditions against.</param>
        /// <returns>True if this imperative can execute right away, or False if it cannot.</returns>
        public static bool CanExecute(this IImperative imperative, IGameState state)
        {
            // Optionally check required cards for starting the imperative
            if (imperative.Requirements?.IsConditionMet(state) == false)
            {
                return false;
            }

            // Sometimes, we want to stop an imperative if other cards are present.
            if (imperative.Forbidders?.IsConditionMet(state) == true)
            {
                return false;
            }

            if (!imperative.Operation.IsConditionMet(state))
            {
                return false;
            }

            return true;
        }
    }
}
