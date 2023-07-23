namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameResources;
    using AutoccultistNS.GameState;

    /// <summary>
    /// Defines a combination operation reactor / impulse / imperative.
    /// This config object defines both the conditions on which to perform an operation, and the operation itself.
    /// </summary>
    [LibraryConfigObject("operations")]
    public class OperationConfig : OperationReactorConfig, IImperativeConfig, IImpulse
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
            /// </summary>
            CurrentRecipeSatisfied,

            /// <summary>
            /// The operation will ignore recipe conditions when checking to see if it can start.
            /// </summary>
            IgnoreRecipes,
        }

        /// <summary>
        /// Gets or sets the operation that this operation inherits from.
        /// </summary>
        public OperationConfig Extends { get; set; }

        /// <summary>
        /// Gets or sets the priority for this operation.
        /// Operations with a higher priority will run before lower priority operation.
        /// </summary>
        /// <remarks>
        /// While this operation config will apply this priority to its reactions, it can be overridden  if this operation is
        /// nested inside a <see cref="LeafImperativeConfig"/>.
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
        /// <remarks>
        /// Setting this to true will gate the operation start behind
        /// having a solution to the current recipe of the ongoing situation.
        /// This can be bypassed with <see cref="StartCondition"/> <see cref="OperationStartCondition.IgnoreRecipes"/>, although
        /// this will also bypass all card requirements.
        /// </remarks>
        public bool? TargetOngoing { get; set; }

        /// <inheritdoc/>
        TaskPriority IImpulse.Priority => this.Priority ?? this.Extends?.Priority ?? TaskPriority.Normal;

        /// <inheritdoc/>
        public IReadOnlyCollection<IImperative> Children => new IImperative[0];

        /// <inheritdoc/>
        public override ConditionResult IsConditionMet(IGameState state)
        {
            // Check if our reaction conditions are met.
            var baseConditionMet = base.IsConditionMet(state);
            if (!baseConditionMet)
            {
                return baseConditionMet;
            }

            // Check if our impulse conditions are met.
            return CacheUtils.Compute(this, nameof(this.IsConditionMet), state, () =>
            {
                var situationId = this.GetSituationId();
                var situation = state.Situations.FirstOrDefault(x => x.SituationId == situationId);

                if (situation == null)
                {
                    return SituationConditionResult.ForFailure(situationId, "Situation not found.");
                }

                if (!situation.IsAvailable())
                {
                    return SituationConditionResult.ForFailure(situationId, "Situation is already reserved.");
                }

                var targetOngoing = this.TargetOngoing ?? this.Extends?.TargetOngoing ?? false;
                var startCondition = this.StartCondition ?? this.Extends?.StartCondition ?? OperationStartCondition.AllRecipesSatisified;

                if (targetOngoing != situation.IsOccupied)
                {
                    return SituationConditionResult.ForFailure(situationId, $"Situation is {(situation.IsOccupied ? "occupied" : "idle")}.");
                }

                if (startCondition == OperationStartCondition.IgnoreRecipes)
                {
                    return ConditionResult.Success;
                }

                // Special condition for targetOngoing: We must have a recipe for the current ongoing verb.
                if (targetOngoing && this.GetRecipeSolution(situation, state) == null)
                {
                    return SituationConditionResult.ForFailure(situationId, $"Situation is ongoing but we do not have a solution to its current recipe.");
                }

                var startingRecipe = this.GetStartingRecipe();
                var ongoingRecipes = this.GetOngoingRecipes();

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

                    // Note: we do not check conditional recipes, as they are often overlapping cases or target nonoverlapping conditions.
                    requiredCards.ToArray().ChooseAll(state.TabletopCards, state, out var unsatisfiedChoice);
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

                    recipeSolution.GetRequiredCards().ChooseAll(state.TabletopCards, state, out var unsatisfiedChoice);
                    if (unsatisfiedChoice != null)
                    {
                        return AddendedConditionResult.Addend(CardChoiceResult.ForFailure(unsatisfiedChoice), $"when ensuring current recipe can start");
                    }
                }

                return ConditionResult.Success;
            });
        }

        /// <inheritdoc/>
        public ConditionResult IsSatisfied(IGameState state)
        {
            return AddendedConditionResult.Addend(ConditionResult.Failure, "Operation imperatives are never satisfied.");
        }

        /// <inheritdoc/>
        public IEnumerable<string> DescribeCurrentGoals(IGameState state)
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<IImperative> GetActiveChildren(IGameState state)
        {
            return Enumerable.Empty<IImperative>();
        }

        /// <inheritdoc/>
        public IEnumerable<IImpulse> GetImpulses(IGameState state)
        {
            if (this.IsConditionMet(state))
            {
                yield return this;
            }
        }

        /// <inheritdoc/>
        protected override string GetSituationId()
        {
            return base.GetSituationId() ?? this.Extends?.Situation;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
