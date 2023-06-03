namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// An operation is a series of tasks to complete a verb or situation.
    /// </summary>
    public class OperationConfig : INamedConfigObject, IGameStateCondition, IOperation, IAfterYamlDeserialization
    {
        /// <summary>
        /// Defines options for when to consider this operation startable.
        /// </summary>
        public enum OperationStartCondition
        {
            /// <summary>
            /// The operation can only start if both the starting and ongoing recipe solutions can be satisfied.
            /// Note: conditionalOngoingRecipes are not included in this check.
            /// This is the default option.
            /// </summary>
            AllRecipesSatisified,

            /// <summary>
            /// The operation can start if either the starting recipe (if the situation is idle) or a matched ongoing recipe can be satisfied.
            /// Note: conditionalOngoingRecipes will be checked if no ongoingRecipe matches the current recipe.
            CurrentRecipeSatisfied,
        }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the situation id to target for this operation.
        /// </summary>
        public string Situation { get; set; }

        /// <summary>
        /// Gets or sets the condition for when this operation should start.
        /// </summary>
        // TODO: We confuse 'condition' here with IGameStateCondition.
        // ...Maybe we should overload this in the parser and allow either.
        public OperationStartCondition StartCondition { get; set; } = OperationStartCondition.AllRecipesSatisified;

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

        /// <summary>
        /// Gets or sets a list of conditional recipes to use for ongoing operations.
        /// </summary>
        public List<ConditionalRecipeSolutionConfig> ConditionalOngoingRecipes { get; set; } = new List<ConditionalRecipeSolutionConfig>();

        /// <inheritdoc/>
        IRecipeSolution IOperation.StartingRecipe => this.StartingRecipe;

        /// <inheritdoc/>
        // IReadOnlyDictionary is not marked with out params...
        IReadOnlyDictionary<string, IRecipeSolution> IOperation.OngoingRecipes => this.OngoingRecipes.ToDictionary(entry => entry.Key, entry => entry.Value as IRecipeSolution);

        IReadOnlyList<IConditionalRecipeSolution> IOperation.ConditionalOngoingRecipes => this.ConditionalOngoingRecipes.ToList().AsReadOnly();

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

        ConditionResult IGameStateCondition.IsConditionMet(IGameState state)
        {
            var situation = state.Situations.FirstOrDefault(x => x.SituationId == this.Situation);
            if (situation == null)
            {
                return new SituationConditionFailure(this.Situation, "Situation not found.");
            }

            if (this.TargetOngoing != situation.IsOccupied)
            {
                return new SituationConditionFailure(this.Situation, $"Situation is {(situation.IsOccupied ? "ongoing" : "idle")}.");
            }

            if (this.StartCondition == OperationStartCondition.AllRecipesSatisified)
            {
                // We currently take advantage of only ever having RecipeSolutionConfigs and rely on knowledge of
                //  their optional cards.  This may be problematic if we ever get other types of recipe solutions.
                IEnumerable<ICardChooser> requiredCards = new ICardChooser[0];

                if (!this.TargetOngoing && this.StartingRecipe != null)
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

                if (!state.CardsCanBeSatisfied(requiredCards.ToArray(), out var unsatisfiedChoice))
                {
                    return new AddendedConditionFailure(new CardChoiceNotSatisfiedFailure(unsatisfiedChoice), $"when ensuring all recipes can start");
                }
            }
            else if (this.StartCondition == OperationStartCondition.CurrentRecipeSatisfied)
            {
                IRecipeSolution recipeSolution;
                if (situation.State == SecretHistories.Enums.StateEnum.Unstarted)
                {
                    recipeSolution = this.StartingRecipe;
                }
                else
                {
                    recipeSolution = this.GetOngoingRecipeSolution(situation);
                }

                if (!state.CardsCanBeSatisfied(recipeSolution.GetRequiredCards(), out var unsatisfiedChoice))
                {
                    return new AddendedConditionFailure(new CardChoiceNotSatisfiedFailure(unsatisfiedChoice), $"when ensuring current recipe can start");
                }
            }

            return ConditionResult.Success;
        }
    }
}
