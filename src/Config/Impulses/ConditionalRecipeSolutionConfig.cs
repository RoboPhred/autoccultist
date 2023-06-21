namespace AutoccultistNS.Config
{
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config.Conditions;
    using AutoccultistNS.GameState;

    /// <summary>
    /// Defines a recipe solution that is only valid if a condition is met.
    /// </summary>
    public class ConditionalRecipeSolutionConfig : RecipeSolutionConfig, IConditionalRecipeSolution
    {
        /// <summary>
        /// Gets or sets the condition that must be met for this solution to be used.
        /// If not set, no extra conditions will be applied.
        /// </summary>
        public IGameStateConditionConfig Condition { get; set; }

        /// <sumary>
        /// Gets or sets whether this solution should be used even if its slot choices cannot be matched.
        /// defaults to `false`.
        /// </summary>
        public bool MatchIfCardsMissing { get; set; } = false;

        ConditionResult IGameStateCondition.IsConditionMet(IGameState state)
        {
            if (!this.MatchIfCardsMissing && !state.CardsCanBeSatisfied(this.GetRequiredCards(), out var unsatisfiedChoice))
            {
                return CardChoiceResult.ForFailure(unsatisfiedChoice);
            }

            if (this.Condition != null)
            {
                return this.Condition.IsConditionMet(state);
            }

            return ConditionResult.Success;
        }
    }
}
