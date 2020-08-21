namespace Autoccultist.Config.Conditions
{
    using System.Collections.Generic;
    using Autoccultist.GameState;
    using Autoccultist.Yaml;

    /// <summary>
    /// A node that can contain any condition operating against a list of cards.
    /// </summary>
    [DuckTypeCandidate(typeof(CardSetCondition))]
    [DuckTypeCandidate(typeof(CardChoiceCondition))]
    public interface ICardConditionConfig : IGameStateConditionConfig
    {
        /// <summary>
        /// Determine if the given cards match against the card set.
        /// </summary>
        /// <param name="cards">The cards to match against the card set.</param>
        /// <returns>True if the card set is satsified by the cards in the given list, False otherwise.</returns>
        bool CardsMatchSet(IReadOnlyCollection<ICardState> cards);
    }
}
