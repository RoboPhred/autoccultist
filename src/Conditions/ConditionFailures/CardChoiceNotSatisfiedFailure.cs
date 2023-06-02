namespace AutoccultistNS
{
    /// <summary>
    /// Describes a failure to satisfy a card chooser.
    /// </summary>
    public class CardChoiceNotSatisfiedFailure : ConditionFailure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardChoiceNotSatisfiedFailure"/> class.
        /// </summary>
        /// <param name="chooser">The card chooser that could not be satisfied.</param>
        public CardChoiceNotSatisfiedFailure(ICardChooser chooser)
        {
            this.Chooser = chooser;
        }

        /// <summary>
        /// Gets the card chooser that could not be satisfied.
        /// </summary>
        public ICardChooser Chooser { get; private set; }

        public override string ToString()
        {
            return $"Could not satisfy card chooser {this.Chooser}.";
        }
    }
}
