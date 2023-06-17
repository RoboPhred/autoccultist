namespace AutoccultistNS.Config.Conditions
{
    using System.Collections.Generic;
    using AutoccultistNS.GameState;
    using YamlDotNet.Core;

    /// <summary>
    /// A set of CardChoice matchers.  Any card existing will satisfy the match.
    /// </summary>
    public class AnyCardExistsCondition : ConditionConfig, ICardConditionConfig
    {
        /// <summary>
        /// Gets or sets a list of cards, any of which may be present to meet this condition.
        /// </summary>
        public List<CardExistsCondition> AnyOf { get; set; } = new List<CardExistsCondition>();

        /// <inheritdoc/>
        public override ConditionResult IsConditionMet(IGameState state)
        {
            return this.CardsMatchSet(state.GetAllCards());
        }

        /// <inheritdoc/>
        public ConditionResult CardsMatchSet(IEnumerable<ICardState> cards)
        {
            var failures = new List<ConditionResult>();
            foreach (var chooser in this.AnyOf)
            {
                // TODO: Each chooser individually chooses a card, so its possible for a chooser
                // to have multiple choices, but choose the one that is the only viable card for another chooser.
                // We should get all candidates for all choosers and try to satisfy them all.
                var choice = chooser.ChooseCard(cards);
                if (choice != null)
                {
                    return ConditionResult.Success;
                }
                else
                {
                    failures.Add(new CardChoiceNotSatisfiedFailure(chooser));
                }
            }

            return new AddendedConditionFailure(new CompoundConditionFailure(failures), $"when looking for any of {this.AnyOf.Count} cards");
        }

        /// <inheritdoc/>
        protected override void OnAfterDeserialized(Mark start, Mark end)
        {
            if (this.AnyOf == null || this.AnyOf.Count == 0)
            {
                throw new InvalidConfigException("AnyCardExistsCondition must have card choices.");
            }

            base.OnAfterDeserialized(start, end);
        }
    }
}