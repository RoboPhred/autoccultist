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
            var choicesByChooser = choosers.ToDictionary(c => c, c => new List<ICardState>(c.SelectChoices(cards)));

            // The sorting is a bit heavy, so try to early-out if we can
            unsatisfiedChooser = choicesByChooser.FirstOrDefault(p => p.Value.Count == 0).Key;
            if (unsatisfiedChooser != null)
            {
                return null;
            }

            var candidates = choicesByChooser.SelectMany(c => c.Value).Distinct();

            // FIXME: Value.Contains is being done on a list, because we want to preserve priority.  We were using a HashSet
            // for efficiency, but that loses the priority information.  We should probably use a SortedSet instead.
            var weightByCandidate = candidates.ToDictionary(c => c, c => choicesByChooser.Sum(p => p.Value.Contains(c) ? 1 : 0));

            var choices = new Dictionary<ICardChooser, ICardState>();

            foreach (var pair in choicesByChooser)
            {
                // FIXME: Value.IndexOf is inefficient.  Use SortedSet
                var choice = pair.Value.Where(c => weightByCandidate.ContainsKey(c)).OrderBy(c => weightByCandidate[c]).ThenBy(c => pair.Value.IndexOf(c)).FirstOrDefault();
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
