namespace AutoccultistNS.Config.Conditions
{
    using System.Collections.Generic;
    using AutoccultistNS.GameState;

    /// <summary>
    /// Represents a choice of a card based on various attributes.
    /// </summary>
    public class CardExistsCondition : CardChooserConfig, ICardConditionConfig
    {
        /// <inheritdoc/>
        public virtual ConditionResult IsConditionMet(IGameState state)
        {
            if (this.ChooseCard(state.GetAllCards()) == null)
            {
                return new CardChoiceNotSatisfiedFailure(this);
            }

            return ConditionResult.Success;
        }

        /// <inheritdoc/>
        public ConditionResult CardsMatchSet(IEnumerable<ICardState> cards)
        {
            var card = this.ChooseCard(cards);
            if (card == null)
            {
                return new CardChoiceNotSatisfiedFailure(this);
            }

            return ConditionResult.Success;
        }
    }
}
