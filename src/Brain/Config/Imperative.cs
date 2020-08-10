using System.Collections.Generic;
using System.Linq;

namespace Autoccultist.Brain.Config
{
    public class Imperative
    {
        public string Name { get; set; }
        public string Situation { get; set; }
        public ImperativePriority Priority { get; set; }

        public GameStateCondition RequiredCards { get; set; }
        public GameStateCondition ForbidWhenCardsPresent { get; set; }

        public RecipeSolution StartingRecipe { get; set; }
        public Dictionary<string, RecipeSolution> OngoingRecipes { get; set; }

        public bool CanExecute(IGameState state)
        {
            if (!state.SituationIsAvailable(this.Situation))
            {
                return false;
            }

            // Optionally check required cards for starting the imperative
            if (this.RequiredCards != null)
            {
                if (!this.RequiredCards.IsConditionMet(state))
                {
                    return false;
                }
            }

            if (this.ForbidWhenCardsPresent != null)
            {
                // Sometimes, we want to stop an imperative if other cards are present.
                if (this.ForbidWhenCardsPresent.IsConditionMet(state))
                {
                    return false;
                }
            }

            // TODO: Have a way for cards to be optional - not required when checking if this imperative should execute.


            // Ideally, IDictionary.Values would be a IReadOnlyCollection, but it predates that interface.
            if (this.StartingRecipe != null)
            {
                var cardMatchers = this.StartingRecipe.Slots.Values.Cast<ICardMatcher>().ToArray();
                if (!state.CardsCanBeSatisfied(cardMatchers))
                {
                    return false;
                }
            }

            if (this.OngoingRecipes != null)
            {
                foreach (var ongoing in this.OngoingRecipes.Values)
                {
                    var cardMatchers = ongoing.Slots.Values.Cast<ICardMatcher>().ToArray();
                    if (!state.CardsCanBeSatisfied(cardMatchers))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }
}