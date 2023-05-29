namespace AutoccultistNS.GameState
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extensions to <see cref="ISituationState"/>.
    /// </summary>
    public static class SituationStateExtensions
    {
        public static IEnumerable<ICardState> GetSlottedCards(this ISituationState situationState)
        {
            return situationState.RecipeSlots.Select(x => x.Card).Where(x => x != null);
        }

        /// <summary>
        /// Get all aspects contained within the situation.
        /// </summary>
        /// <param name="situationState">The situation to determine aspects for.</param>
        /// <returns>A dictionary of aspect names to degrees.</returns>
        public static IReadOnlyDictionary<string, int> GetAspects(this ISituationState situationState)
        {
            var degreesByAspect =
                from card in situationState.GetSlottedCards().Concat(situationState.StoredCards)
                from aspectEntry in card.Aspects
                group aspectEntry by aspectEntry.Key into aspectGroup
                select new { Aspect = aspectGroup.Key, Degree = aspectGroup.Sum(x => x.Value) };

            return degreesByAspect.ToDictionary(entry => entry.Aspect, entry => entry.Degree);
        }
    }
}
