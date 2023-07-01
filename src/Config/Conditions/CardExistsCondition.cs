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
        public ConditionResult IsConditionMet(IGameState state)
        {
            if (this.ChooseCard(state.AllCards, state) == null)
            {
                return CardChoiceResult.ForFailure(this);
            }

            return ConditionResult.Success;
        }

        /// <inheritdoc/>
        public ConditionResult CardsMatchSet(IEnumerable<ICardState> cards, IGameState state)
        {
            var card = this.ChooseCard(cards, state);
            if (card == null)
            {
                return CardChoiceResult.ForFailure(this);
            }

            return ConditionResult.Success;
        }
    }
}
