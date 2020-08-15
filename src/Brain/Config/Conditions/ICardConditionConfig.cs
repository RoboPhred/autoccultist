﻿namespace Autoccultist.Brain.Config.Conditions
{
    using System.Collections.Generic;
    using Assets.Core.Interfaces;
    using Autoccultist.Yaml;

    /// <summary>
    /// A node that can contain any condition operating against a list of cards.
    /// </summary>
    [DuckTypeCandidate(typeof(CardSetCondition))]
    [DuckTypeCandidate(typeof(CardChoice))]
    public interface ICardConditionConfig : IGameStateConditionConfig
    {
        /// <summary>
        /// Determine if the given cards match against the card set.
        /// </summary>
        /// <param name="cards">The cards to match against the card set.</param>
        /// <returns>True if the card set is satsified by the cards in the given list, False otherwise.</returns>
        bool CardsMatchSet(IList<IElementStack> cards);
    }
}