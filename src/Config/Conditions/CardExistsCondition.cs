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
        public virtual bool IsConditionMet(IGameState state)
        {
            return this.ChooseCard(state.GetAllCards()) != null;
        }

        /// <inheritdoc/>
        public bool CardsMatchSet(IEnumerable<ICardState> cards)
        {
            return this.ChooseCard(cards) != null;
        }
    }
}
