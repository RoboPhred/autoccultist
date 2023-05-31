namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config.CardChoices;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// An operation is a series of tasks to complete a verb or situation.
    /// </summary>
    public class OperationConfig : INamedConfigObject, IGameStateCondition, IOperation, IAfterYamlDeserialization
    {
        /// <inheritdoc/>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the situation id to target for this operation.
        /// </summary>
        public string Situation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this operation should target an ongoign situation.
        /// </summary>
        public bool TargetOngoing { get; set; }

        /// <summary>
        /// Gets or sets the recipe used to start this situation.
        /// </summary>
        public RecipeSolutionConfig StartingRecipe { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of recipe ids to recipe solutions for each ongoing recipe the situation may encounter.
        /// </summary>
        public Dictionary<string, RecipeSolutionConfig> OngoingRecipes { get; set; } = new Dictionary<string, RecipeSolutionConfig>();

        /// <inheritdoc/>
        IRecipeSolution IOperation.StartingRecipe => this.StartingRecipe;

        /// <inheritdoc/>
        // IReadOnlyDictionary is not marked with out params...
        IReadOnlyDictionary<string, IRecipeSolution> IOperation.OngoingRecipes => this.OngoingRecipes.ToDictionary(entry => entry.Key, entry => entry.Value as IRecipeSolution);

        /// <summary>
        /// Determines if this operation is able to execute given the supplied game state.
        /// </summary>
        /// <param name="state">The game state to match conditions against.</param>
        /// <returns>True if this operation is able to execute at this moment, or False otherwise.</returns>
        public bool IsConditionMet(IGameState state)
        {
            var situation = state.Situations.FirstOrDefault(x => x.SituationId == this.Situation);
            if (situation == null)
            {
                return false;
            }

            if (this.TargetOngoing != situation.IsOccupied)
            {
                return false;
            }

            // We currently take advantage of only ever having RecipeSolutionConfigs and rely on knowledge of
            //  their optional cards.  This may be problematic if we ever get other types of recipe solutions.
            IEnumerable<ICardChooser> requiredCards = new ICardChooser[0];

            if (this.StartingRecipe != null)
            {
                requiredCards = requiredCards.Concat(this.StartingRecipe.GetRequiredCards());
            }

            if (this.OngoingRecipes != null)
            {
                requiredCards = requiredCards.Concat(
                    from ongoing in this.OngoingRecipes.Values
                    from choice in ongoing.GetRequiredCards()
                    select choice);
            }

            // TODO: Optional card slots.
            // Do not require optional cards to be satisfied.
            // requiredCards = requiredCards.Where(x => !x.Optional);
            return state.CardsCanBeSatisfied(requiredCards.ToArray());
        }

        /// <inheritdoc/>
        public void AfterDeserialized(Mark start, Mark end)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            if (string.IsNullOrEmpty(this.Situation))
            {
                throw new InvalidConfigException($"Operation {this.Name} must have a situation.");
            }
        }
    }
}
