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
            return this.CardsMatchSet(state.GetAllCards());
        }

        /// <inheritdoc/>
        public ConditionResult CardsMatchSet(IEnumerable<ICardState> cards)
        {
            var remaining = new HashSet<ICardState>(cards);
            foreach (var chooser in this.CardSet)
            {
                // TODO: Each chooser individually chooses a card, so its possible for a chooser
                // to have multiple choices, but choose the one that is the only viable card for another chooser.
                // We should get all candidates for all choosers and try to satisfy them all.
                var choice = chooser.ChooseCard(remaining);
                if (choice == null)
                {
                    return AddendedConditionResult.Addend(CardChoiceResult.ForFailure(chooser), $"when looking for a set of {this.CardSet.Count} cards");
                }

                remaining.Remove(choice);
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
