namespace AutoccultistNS
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameState;

    public static class ICardChooserExtensions
    {
        /// <summary>
        /// Choose a card from the given candidates.
        /// </summary>
        /// <param name="chooser">The chooser to use.</param>
        /// <param name="cards">An enumerable of candidate cards to choose from.</param>
        /// <returns>The highest priority card chosen.</returns>
        public static ICardState ChooseCard(this ICardChooser chooser, IEnumerable<ICardState> cards)
        {
            return chooser.SelectChoices(cards).FirstOrDefault();
        }

        public static IReadOnlyDictionary<ICardChooser, ICardState> ChooseAll(this IEnumerable<ICardChooser> choosers, IEnumerable<ICardState> cards, out ICardChooser unsatisfiedChooser)
        {
            var choicesByChooser = choosers.ToDictionary(c => c, c => new HashSet<ICardState>(c.SelectChoices(cards)));

            // The sorting is a bit heavy, so try to early-out if we can
            unsatisfiedChooser = choicesByChooser.FirstOrDefault(p => p.Value.Count == 0).Key;
            if (unsatisfiedChooser != null)
            {
                return null;
            }

            var candidates = choicesByChooser.SelectMany(c => c.Value).Distinct();
            var weightByCandidate = candidates.ToDictionary(c => c, c => choicesByChooser.Sum(p => p.Value.Contains(c) ? 1 : 0));

            var choices = new Dictionary<ICardChooser, ICardState>();

            // Start with the most restrictive choices first, and work our way down.
            foreach (var pair in choicesByChooser.OrderBy(p => p.Value.Count))
            {
                var choice = pair.Value.Where(c => weightByCandidate.ContainsKey(c)).OrderBy(c => weightByCandidate[c]).FirstOrDefault();
                if (choice == null)
                {
                    unsatisfiedChooser = pair.Key;
                    return null;
                }

                // Remove the choice from the options
                weightByCandidate.Remove(choice);

                // Mark the card as chosen
                choices.Add(pair.Key, choice);
            }

            return choices;
        }
    }
}
