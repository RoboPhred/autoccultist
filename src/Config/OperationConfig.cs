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
    public class OperationConfig : NamedConfigObject, IGameStateCondition, IOperation
    {
        private IReadOnlyDictionary<string, IRecipeSolution> ongoingRecipes;

        private IReadOnlyList<IConditionalRecipeSolution> conditionalOngoingRecipes;

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

        public OperationConfig Extends { get; set; }

        /// <summary>
        /// Gets or sets the situation id to target for this operation.
        /// </summary>
        public string Situation { get; set; }

        /// <summary>
        /// Gets or sets the condition for when this operation should start.
        /// </summary>
        // TODO: We confuse 'condition' here with IGameStateCondition.
        // ...Maybe we should overload this in the parser and allow either.
        public OperationStartCondition? StartCondition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this operation should target an ongoign situation.
        /// </summary>
        public bool? TargetOngoing { get; set; }

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

        string IOperation.Situation => this.Situation ?? this.Extends?.Situation;

        /// <inheritdoc/>
        IRecipeSolution IOperation.StartingRecipe => this.StartingRecipe ?? this.Extends?.StartingRecipe;

        /// <inheritdoc/>
        IReadOnlyDictionary<string, IRecipeSolution> IOperation.OngoingRecipes
        {
            get
            {
                if (this.ongoingRecipes == null)
                {
                    var extended = this.Extends?.OngoingRecipes ?? new Dictionary<string, RecipeSolutionConfig>();
                    var self = this.OngoingRecipes ?? new Dictionary<string, RecipeSolutionConfig>();

                    // Extended comes first, self second.  This way, our current op overrides extended ops.
                    this.ongoingRecipes = extended.Merge(self).ToDictionary(x => x.Key, x => (IRecipeSolution)x.Value);
                }

                return this.ongoingRecipes;
            }
        }

        /// <inheritdoc/>
        IReadOnlyList<IConditionalRecipeSolution> IOperation.ConditionalOngoingRecipes
        {
            get
            {
                if (this.conditionalOngoingRecipes == null)
                {
                    // Our conditionals come first so they take priority.
                    this.conditionalOngoingRecipes = this.ConditionalOngoingRecipes.Concat(this.Extends?.ConditionalOngoingRecipes ?? new List<ConditionalRecipeSolutionConfig>()).ToList().AsReadOnly();
                }

                return this.conditionalOngoingRecipes;
            }
        }

        /// <inheritdoc/>
        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            if (string.IsNullOrEmpty(this.Situation) && string.IsNullOrEmpty(this.Extends?.Situation))
            {
                throw new InvalidConfigException($"Operation {this.Name} must have a situation.");
            }
        }

        ConditionResult IGameStateCondition.IsConditionMet(IGameState state)
        {
            var selfAsInterface = (IOperation)this;
            var situationId = selfAsInterface.Situation;
            var startingRecipe = selfAsInterface.StartingRecipe;
            var ongoingRecipes = selfAsInterface.OngoingRecipes;

            var targetOngoing = this.TargetOngoing ?? this.Extends?.TargetOngoing ?? false;
            var startCondition = this.StartCondition ?? this.Extends?.StartCondition ?? OperationStartCondition.AllRecipesSatisified;

            var situation = state.Situations.FirstOrDefault(x => x.SituationId == situationId);
            if (situation == null)
            {
                return new SituationConditionFailure(situationId, "Situation not found.");
            }

            if (targetOngoing != situation.IsOccupied)
            {
                return new SituationConditionFailure(situationId, $"Situation is {(situation.IsOccupied ? "ongoing" : "idle")}.");
            }

            if (startCondition == OperationStartCondition.AllRecipesSatisified)
            {
                // We currently take advantage of only ever having RecipeSolutionConfigs and rely on knowledge of
                //  their optional cards.  This may be problematic if we ever get other types of recipe solutions.
                IEnumerable<ICardChooser> requiredCards = new ICardChooser[0];

                if (!targetOngoing && startingRecipe != null)
                {
                    requiredCards = requiredCards.Concat(startingRecipe.GetRequiredCards());
                }

                if (ongoingRecipes != null)
                {
                    requiredCards = requiredCards.Concat(
                        from ongoing in ongoingRecipes.Values
                        from choice in ongoing.GetRequiredCards()
                        select choice);
                }

                if (!state.CardsCanBeSatisfied(requiredCards.ToArray(), out var unsatisfiedChoice))
                {
                    return new AddendedConditionFailure(new CardChoiceNotSatisfiedFailure(unsatisfiedChoice), $"when ensuring all recipes can start");
                }
            }
            else if (startCondition == OperationStartCondition.CurrentRecipeSatisfied)
            {
                IRecipeSolution recipeSolution;
                if (situation.State == SecretHistories.Enums.StateEnum.Unstarted)
                {
                    recipeSolution = startingRecipe;
                }
                else
                {
                    recipeSolution = this.GetCurrentRecipeSolution(situation);
                }

                if (recipeSolution == null)
                {
                    return new SituationConditionFailure(situationId, $"Can not handle the current recipe {situation.CurrentRecipe ?? "<start>"}");
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
