using System.Collections.Generic;
using System.Linq;

namespace Autoccultist.Brain.Config
{
    public class Operation
    {
        public string Name { get; set; }
        public string Situation { get; set; }

        public RecipeSolution StartingRecipe { get; set; }
        public Dictionary<string, RecipeSolution> OngoingRecipes { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(this.Situation))
            {
                throw new InvalidConfigException($"Operation {this.Name} must have a situation.");
            }
        }

        public bool CanExecute(IGameState state)
        {
            if (!state.SituationIsAvailable(this.Situation))
            {
                return false;
            }

            IEnumerable<CardChoice> requiredCards = new CardChoice[0];

            // We need to ensure all cards are available, including ongoing.

            //  TODO: More complex and long running situations may not allow us to have all we need right at the start
            //  We should be able to have optional card choices that do not count to starting.
            if (this.StartingRecipe != null)
            {
                requiredCards = requiredCards.Concat(this.StartingRecipe.Slots.Values);
            }

            if (this.OngoingRecipes != null)
            {
                requiredCards = requiredCards.Concat(
                    from ongoing in this.OngoingRecipes.Values
                    from choice in ongoing.Slots.Values
                    select choice
                );
            }

            return state.CardsCanBeSatisfied(requiredCards.ToArray());
        }
    }
}