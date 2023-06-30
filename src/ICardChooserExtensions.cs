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
        /// <param name="state">The current game state.</param>
        /// <returns>The highest priority card chosen.</returns>
        public static ICardState ChooseCard(this ICardChooser chooser, IEnumerable<ICardState> cards, IGameState state)
        {
            return chooser.SelectChoices(cards, state).FirstOrDefault();
        }

        /// <summary>
        /// Finds the best choice for all card choosers such that all choosers are satisfied.
        /// </summary>
        /// <param name="choosers">The choosers to use.</param>
        /// <param name="cards">An enumerable of candidate cards to choose from.</param>
        /// <param name="state">The current game state.</param>
        /// <param name="unsatisfiedChooser">The chooser that was not satisfied, if any.</param>
        /// <returns>A dictionary of choosers to chosen cards, or null if no choice was possible.</returns>
        public static IReadOnlyDictionary<ICardChooser, ICardState> ChooseAll(this IEnumerable<ICardChooser> choosers, IEnumerable<ICardState> cards, IGameState state, out ICardChooser unsatisfiedChooser)
        {
            var choicesByChooser = choosers.ToDictionary(c => c, c => new List<ICardState>(c.SelectChoices(cards, state)));

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
