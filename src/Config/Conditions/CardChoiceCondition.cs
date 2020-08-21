namespace Autoccultist.Config.Conditions
{
    using System.Collections.Generic;
    using Autoccultist.GameState;

    /// <summary>
    /// Represents a choice of a card based on various attributes.
    /// </summary>
    public class CardChoiceCondition : CardChoiceConfig, ICardConditionConfig
    {
        /// <inheritdoc/>
        public virtual bool IsConditionMet(IGameState state)
        {
            return this.ChooseCard(state.TabletopCards) != null;
        }

        /// <inheritdoc/>
        public bool CardsMatchSet(IReadOnlyCollection<ICardState> cards)
        {
            return this.ChooseCard(cards) != null;
        }
    }
}
