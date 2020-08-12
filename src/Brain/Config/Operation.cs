using System.Collections.Generic;

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

            if (this.StartingRecipe != null && !this.StartingRecipe.CanExecute(state))
            {
                return false;
            }

            if (this.OngoingRecipes != null)
            {
                foreach (var ongoing in this.OngoingRecipes.Values)
                {
                    // TODO: Should allow these to late-resolve their cards instead of requiring them up-front.
                    //  maybe a allowIncomplete: true in RecipeSolution itself, or on the CardMatcher
                    if (!ongoing.CanExecute(state))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }
}