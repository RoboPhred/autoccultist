using System.Collections.Generic;
using System.Linq;

namespace Autoccultist.Brain.Config
{
    public class Imperative
    {
        public string SituationID;
        public ImperativePriority Priority;

        public RecipeSolution StartingRecipeSolution;
        public IDictionary<string, RecipeSolution> OngoingRecipeSolutions;

        public bool CanExecute(IGameState state)
        {
            if (!state.SituationIsAvailable(this.SituationID))
            {
                return false;
            }

            // Ideally, IDictionary.Values would be a IReadOnlyCollection, but it predates that interface.
            if (!state.CardsCanBeSatisfied(this.StartingRecipeSolution.SlotSolutions.Values.Cast<ICardMatcher>().ToArray()))
            {
                return false;
            }
            return true;
        }
    }
}