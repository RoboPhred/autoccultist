namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Resources;

    /// <summary>
    /// An operation is a series of tasks to complete a verb or situation.
    /// </summary>
    public class OperationImpulseConfig : OperationConfig, IImpulseConfig
    {
        /// <summary>
        /// Defines options for when to consider this operation startable.
        /// </summary>
        public enum OperationStartCondition
        {
            /// <summary>
            /// The operation can only start if both the starting and ongoing recipe solutions can be satisfied.
            /// Note: conditionalRecipes are not included in this check.
            /// This is the default option.
            /// </summary>
            AllRecipesSatisified,

            /// <summary>
            /// The operation can start if either the starting recipe (if the situation is idle) or a matched ongoing recipe can be satisfied.
            /// Note: conditionalRecipes will be checked if no ongoingRecipe matches the current recipe.
            CurrentRecipeSatisfied,
        }

        /// <summary>
        /// Gets or sets the operation that this operation inherits from.
        /// </summary>
        public OperationImpulseConfig Extends { get; set; }

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
        /// Gets or sets a value indicating whether this operation should target an ongoing situation.
        /// </summary>
        public bool? TargetOngoing { get; set; }

        TaskPriority IImpulse.Priority => this.Priority ?? this.Extends?.Priority ?? TaskPriority.Normal;

        public ConditionResult IsConditionMet(IGameState state)
        {
            var situationId = this.GetSituationId();
            var situationState = state.Situations.FirstOrDefault(x => x.SituationId == situationId);

            if (situationState == null)
            {
                return SituationConditionResult.ForFailure(situationId, "Situation not found.");
            }

            if (!Resource.Of<ISituationState>().IsAvailable(situationState))
            {
                return SituationConditionResult.ForFailure(situationId, "Situation is already reserved.");
            }

            var startingRecipe = this.GetStartingRecipe();
            var ongoingRecipes = this.GetOngoingRecipes();

            var targetOngoing = this.TargetOngoing ?? this.Extends?.TargetOngoing ?? false;
            var startCondition = this.StartCondition ?? this.Extends?.StartCondition ?? OperationStartCondition.AllRecipesSatisified;

            var situation = state.Situations.FirstOrDefault(x => x.SituationId == situationId);
            if (situation == null)
            {
                return SituationConditionResult.ForFailure(situationId, "Situation not found.");
            }

            if (targetOngoing != situation.IsOccupied)
            {
                return SituationConditionResult.ForFailure(situationId, $"Situation is {(situation.IsOccupied ? "ongoing" : "idle")}.");
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

                requiredCards.ToArray().ChooseAll(state.TabletopCards, out var unsatisfiedChoice);
                if (unsatisfiedChoice != null)
                {
                    return AddendedConditionResult.Addend(CardChoiceResult.ForFailure(unsatisfiedChoice), $"when ensuring all recipes can start");
                }
            }
            else if (startCondition == OperationStartCondition.CurrentRecipeSatisfied)
            {
                var recipeSolution = this.GetRecipeSolution(situation);
                if (recipeSolution == null)
                {
                    return SituationConditionResult.ForFailure(situationId, $"Can not handle the current recipe {situation.CurrentRecipe ?? "<start>"}");
                }

                recipeSolution.GetRequiredCards().ChooseAll(state.TabletopCards, out var unsatisfiedChoice);
                if (unsatisfiedChoice != null)
                {
                    return AddendedConditionResult.Addend(CardChoiceResult.ForFailure(unsatisfiedChoice), $"when ensuring current recipe can start");
                }
            }

            return ConditionResult.Success;
        }

        protected override string GetSituationId()
        {
            return base.GetSituationId() ?? this.Extends?.Situation;
        }

        protected override IRecipeSolution GetStartingRecipe()
        {
            var startingRecipe = base.GetStartingRecipe();
            if (startingRecipe != null)
            {
                return startingRecipe;
            }

            if (this.Extends != null)
            {
                return this.Extends.GetStartingRecipe();
            }

            return null;
        }

        protected override IReadOnlyDictionary<string, IRecipeSolution> GetOngoingRecipes()
        {
            var ongoingRecipes = base.GetOngoingRecipes();

            if (this.Extends != null)
            {
                // Extended comes first, self second.  This way, our current op overrides extended ops.
                ongoingRecipes = this.Extends.GetOngoingRecipes().Merge(ongoingRecipes);
            }

            return ongoingRecipes;
        }

        protected override IReadOnlyList<IConditionalRecipeSolution> GetConditionalRecipes()
        {
            // Our conditionals come first so they take priority.
            var conditionalRecipes = base.GetConditionalRecipes();

            if (this.Extends != null)
            {
                conditionalRecipes = conditionalRecipes.Concat(this.Extends.GetConditionalRecipes()).ToList();
            }

            return conditionalRecipes;
        }
    }
}
