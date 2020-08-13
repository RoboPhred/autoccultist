using System.Collections.Generic;
using Autoccultist.GameState;

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
            if (!(state.GetSituation(this.Situation)?.IsBusy ?? false))
            {
                return false;
            }

            // We use the same consumption scope for starting recipe and ongoing recipies, as
            //  each recipe will consume against the next.
            var childState = state.CreateConsumptionScope();

            if (this.StartingRecipe != null && !this.StartingRecipe.TryConsumeCards(childState, out var _))
            {
                return false;
            }

            if (this.OngoingRecipes != null)
            {
                foreach (var ongoing in this.OngoingRecipes.Values)
                {
                    if (!ongoing.TryConsumeCards(childState, out var _))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }
}