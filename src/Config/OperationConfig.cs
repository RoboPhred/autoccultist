namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using SecretHistories.Enums;
    using YamlDotNet.Core;

    /// <summary>
    /// An operation is a series of tasks to complete a verb or situation.
    /// </summary>
    public class OperationConfig : NamedConfigObject, IOperation, IReaction
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

        /// <summary>
        /// Gets or sets the operation that this operation inherits from.
        /// </summary>
        public OperationConfig Extends { get; set; }

        /// <summary>
        /// Gets or sets the situation id to target for this operation.
        /// </summary>
        public string Situation { get; set; }

        /// <summary>
        /// Gets or sets the priority for this operation.
        /// Operations with a higher priority will run before lower priority operation.
        /// </summary>
        /// <remarks>
        /// This will have no effect if this operation is nested inside another reaction such as an <see cref="ImpulseConfig"/>.
        /// </remarks>
        public TaskPriority? Priority { get; set; }

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

        TaskPriority IReaction.Priority => this.Priority ?? this.Extends?.Priority ?? TaskPriority.Normal;

        public override string ToString()
        {
            return $"OperationConfig(Name = {this.Name}, Situation = {this.Situation})";
        }

        public IReactionExecution Execute()
        {
            NoonUtility.Log($"Executing operation {this.Name}.");
            return new OperationReactionExecution(this);
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

        public IRecipeSolution GetRecipeSolution(ISituationState situationState, IGameState gameState = null)
        {
            if (gameState == null)
            {
                gameState = GameStateProvider.Current;
            }

            if (situationState.State == StateEnum.Unstarted)
            {
                return this.GetStartingRecipe() ?? this.GetConditionalRecipes().FirstOrDefault(x => x.IsConditionMet(gameState));
            }

            if (situationState.CurrentRecipe == null)
            {
                return null;
            }

            if (this.OngoingRecipes != null && this.GetOngoingRecipes().TryGetValue(situationState.CurrentRecipe, out var recipe))
            {
                return recipe;
            }

            return this.GetConditionalRecipes().FirstOrDefault(x => x.IsConditionMet(gameState));
        }

        public ConditionResult IsConditionMet(IGameState state)
        {
            // This isn't really relative to game state, but we have to use this here, as IReaction is not specific to situations and
            // NucleusAccumbens can no longer verify this.
            if (!SituationOrchestrator.IsSituationAvailable(this.Situation ?? this.Extends?.Situation))
            {
                return new SituationConditionFailure(this.Situation ?? this.Extends?.Situation, "Situation is busy with another orchestration.");
            }

            var situationId = this.Situation ?? this.Extends?.Situation;
            var startingRecipe = this.GetStartingRecipe();
            var ongoingRecipes = this.GetOngoingRecipes();

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

                var recipeSolution = this.GetRecipeSolution(situation);
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

        private IRecipeSolution GetStartingRecipe()
        {
            if (this.StartingRecipe != null)
            {
                return this.StartingRecipe;
            }

            if (this.Extends != null)
            {
                return this.Extends.GetStartingRecipe();
            }

            return null;
        }

        private IReadOnlyDictionary<string, IRecipeSolution> GetOngoingRecipes()
        {
            IReadOnlyDictionary<string, IRecipeSolution> ongoingRecipes = new Dictionary<string, IRecipeSolution>();

            if (this.OngoingRecipes != null)
            {
                ongoingRecipes = this.OngoingRecipes.ToDictionary(x => x.Key, x => (IRecipeSolution)x.Value);
            }

            if (this.Extends != null)
            {
                // Extended comes first, self second.  This way, our current op overrides extended ops.
                ongoingRecipes = this.Extends.GetOngoingRecipes().Merge(ongoingRecipes);
            }

            return ongoingRecipes;
        }

        private IReadOnlyCollection<IConditionalRecipeSolution> GetConditionalRecipes()
        {
            var conditionalRecipes = new List<IConditionalRecipeSolution>();

            if (this.ConditionalOngoingRecipes != null)
            {
                // Our conditionals come first so they take priority.
                conditionalRecipes.AddRange(this.ConditionalOngoingRecipes);
            }

            if (this.Extends != null)
            {
                conditionalRecipes.AddRange(this.Extends.GetConditionalRecipes());
            }

            return conditionalRecipes;
        }
    }
}
