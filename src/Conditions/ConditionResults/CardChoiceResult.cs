namespace AutoccultistNS
{
    /// <summary>
    /// Describes a failure to satisfy a card chooser.
    /// </summary>
    public class CardChoiceResult : GeneralConditionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardChoiceResult"/> class.
        /// </summary>
        /// <param name="chooser">The card chooser that could not be satisfied.</param>
        protected CardChoiceResult(ICardChooser chooser)
        : base($"Could not satisfy card chooser {chooser}.")
        {
            this.Chooser = chooser;
        }

        /// <summary>
        /// Gets the card chooser that could not be satisfied.
        /// </summary>
        public ICardChooser Chooser { get; private set; }

        /// <summary>
        /// Gets a condition result describing a card choice failure.
        /// </summary>
        public static ConditionResult ForFailure(ICardChooser chooser)
        {
            if (ConditionResult.IsTracing)
            {
                return new CardChoiceResult(chooser);
            }

            return ConditionResult.Failure;
        }
    }
}
