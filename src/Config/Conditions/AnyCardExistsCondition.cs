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
                var choice = chooser.ChooseCard(cards);
                if (choice != null)
                {
                    return ConditionResult.Success;
                }
                else
                {
                    failures.Add(CardChoiceResult.ForFailure(chooser));
                }
            }

            return AddendedConditionResult.Addend(CompoundConditionResult.ForFailure(failures), $"when looking for any of {this.AnyOf.Count} cards");
        }

        /// <inheritdoc/>
        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (this.AnyOf == null || this.AnyOf.Count == 0)
            {
                throw new InvalidConfigException("AnyCardExistsCondition must have card choices.");
            }
        }
    }
}
