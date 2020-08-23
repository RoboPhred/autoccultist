namespace Autoccultist.Config.Conditions
{
    using System.Collections.Generic;
    using Autoccultist.GameState;
    using Autoccultist.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// A set of CardChoice matchers.  All card choices must be matched with no overlap for this condition to pass.
    /// </summary>
    public class CardSetCondition : ICardConditionConfig, IAfterYamlDeserialization
    {
        /// <summary>
        /// Gets or sets a list of cards, all of which must be present at the same time to meet this condition.
        /// <para>
        /// Each choice in this list must match to a different card.  Once a card is chosen by a card choice,
        /// other card choices in the list will be unable to match that same card.
        /// </summary>
        public List<CardChoiceCondition> CardSet { get; set; } = new List<CardChoiceCondition>();

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (this.CardSet == null || this.CardSet.Count == 0)
            {
                throw new InvalidConfigException("CardSet must have card choices.");
            }
        }

        /// <inheritdoc/>
        public bool IsConditionMet(IGameState state)
        {
            return state.CardsCanBeSatisfied(this.CardSet);
        }

        /// <inheritdoc/>
        public bool CardsMatchSet(IReadOnlyCollection<ICardState> cards)
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
                    return false;
                }

                remaining.Remove(choice);
            }

            return true;
        }
    }
}
