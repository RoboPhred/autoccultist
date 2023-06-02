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
        public virtual bool IsConditionMet(IGameState state, out ConditionFailure failureDescription)
        {
            if (this.ChooseCard(state.GetAllCards()) == null)
            {
                failureDescription = new CardChoiceNotSatisfiedFailure(this);
                return false;
            }

            failureDescription = null;
            return true;
        }

        /// <inheritdoc/>
        public bool CardsMatchSet(IEnumerable<ICardState> cards, out ConditionFailure failureDescription)
        {
            var card = this.ChooseCard(cards);
            if (card == null)
            {
                failureDescription = new CardChoiceNotSatisfiedFailure(this);
                return false;
            }

            failureDescription = null;
            return true;
        }
    }
}
