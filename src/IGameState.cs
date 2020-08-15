namespace Autoccultist
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the current state of the game.
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// Determines if the given situation is available for use.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to check.</param>
        /// <returns>True if the situation is available for use, or False if it is otherwise occupied.</returns>
        bool IsSituationAvailable(string situationId);

        /// <summary>
        /// Determines if all card matchers can satisfy their cards.
        /// This takes into account card matchers wanting to consume their cards, so once a matcher
        /// chooses a card, other matchers may not consider that card for their choice.
        /// </summary>
        /// <param name="choices">A collection of card matchers to check cards against.</param>
        /// <returns>True if all card matchers can satisfy their matches, or False otherwise.</returns>
        bool CardsCanBeSatisfied(IReadOnlyCollection<ICardMatcher> choices);
    }
}