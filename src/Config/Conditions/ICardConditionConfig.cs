namespace AutoccultistNS.Config.Conditions
{
    using System.Collections.Generic;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;

    /// <summary>
    /// A node that can contain any condition operating against a list of cards.
    /// </summary>
    [DuckTypeCandidate(typeof(CardSetCondition))]
    [DuckTypeCandidate(typeof(CardExistsCondition))]
    [DuckTypeCandidate(typeof(AnyCardExistsCondition))]
    public interface ICardConditionConfig : IGameStateConditionConfig
    {
        /// <summary>
        /// Determine if the given cards match against the card set.
        /// </summary>
        /// <param name="cards">The cards to match against the card set.</param>
        /// <param name="state">The current game state.</param>
        /// <returns>True if the card set is satsified by the cards in the given list, False otherwise.</returns>
        ConditionResult CardsMatchSet(IEnumerable<ICardState> cards, IGameState state);
    }
}
