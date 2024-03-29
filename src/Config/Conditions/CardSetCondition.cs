namespace AutoccultistNS.Config.Conditions
{
    using System.Collections.Generic;
    using AutoccultistNS.GameState;
    using YamlDotNet.Core;

    /// <summary>
    /// A set of CardChoice matchers.  All card choices must be matched with no overlap for this condition to pass.
    /// </summary>
    public class CardSetCondition : ConditionConfig, ICardConditionConfig
    {
        /// <summary>
        /// Gets or sets a list of cards, all of which must be present at the same time to meet this condition.
        /// <para>
        /// Each choice in this list must match to a different card.  Once a card is chosen by a card choice,
        /// other card choices in the list will be unable to match that same card.
        /// </summary>
        public List<CardExistsCondition> CardSet { get; set; } = new List<CardExistsCondition>();

        /// <inheritdoc/>
        public override ConditionResult IsConditionMet(IGameState state)
        {
            return this.CardsMatchSet(state.AllCards, state);
        }

        /// <inheritdoc/>
        public ConditionResult CardsMatchSet(IEnumerable<ICardState> cards, IGameState state)
        {
            var result = this.CardSet.ChooseAll(cards, state, out var unsatisfiedChooser);
            if (unsatisfiedChooser != null)
            {
                return AddendedConditionResult.Addend(CardChoiceResult.ForFailure(unsatisfiedChooser), $"when looking for a set of {this.CardSet.Count} cards");
            }

            if (result == null)
            {
                return AddendedConditionResult.Addend(ConditionResult.Failure, $"when looking for a set of {this.CardSet.Count} cards");
            }

            return ConditionResult.Success;
        }

        /// <inheritdoc/>
        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (this.CardSet == null || this.CardSet.Count == 0)
            {
                throw new InvalidConfigException("CardSet must have card choices.");
            }
        }
    }
}
