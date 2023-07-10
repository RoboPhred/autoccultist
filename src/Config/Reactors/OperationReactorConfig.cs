namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using AutoccultistNS.GameResources;
    using SecretHistories.Enums;
    using YamlDotNet.Core;

    /// <summary>
    /// An operation is a series of tasks to complete a verb or situation.
    /// </summary>
    public class OperationReactorConfig : NamedConfigObject, IOperation, IReactor
    {
        /// <summary>
        /// Gets or sets the situation id to target for this operation.
        /// </summary>
        public string Situation { get; set; }

        /// <summary>
        /// Gets or sets the recipe used to start this situation.
        /// </summary>
        public RecipeSolutionConfig StartingRecipe { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of recipe ids to recipe solutions for each ongoing recipe the situation may encounter.
        /// </summary>
        public Dictionary<string, RecipeSolutionConfig> OngoingRecipes { get; set; } = new();

        /// <summary>
        /// Gets or sets a list of conditional recipes.  These can trigger at any time, for both starting and ongoing.
        /// </summary>
        public List<ConditionalRecipeSolutionConfig> ConditionalRecipes { get; set; } = new();

        /// <summary>
        /// Gets or sets a list of reactions to perform when the operation starts.
        /// </summary>
        public List<IReactorConfig> OnStart { get; set; } = new();

        /// <summary>
        /// Gets or sets a list of reactions to perform when the operation completes.
        /// </summary>
        public List<IReactorConfig> OnComplete { get; set; } = new();

        // Explicitly implement IOperation to get whatever our overriden situationId may be.
        string IOperation.Situation => this.GetSituationId();

        /// <inheritdoc/>
        public virtual ConditionResult IsConditionMet(IGameState state)
        {
            // We need to implement the bare minimum requirements for this reaction to start.
            // This includes:
            // - The situation must exist.
            // - The situation must not be reserved by another reaction.
            // We do not care about any recipes, since the user might want to start us to take over
            // the situation once it hits a recipe we know about.
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

            return ConditionResult.Success;
        }

        /// <inheritdoc/>
        public IReaction GetReaction()
        {
            IReaction reaction = new OperationReaction(this);

            if (this.OnStart != null && this.OnStart.Count > 0)
            {
                reaction = new CompoundReaction(new[] { reaction }.Concat(this.OnStart.Select(x => x.GetReaction())).ToList());
            }

            if (this.OnComplete != null && this.OnComplete.Count > 0)
            {
                var followup = new CompoundReaction(this.OnComplete.Select(x => x.GetReaction()).ToList());
                reaction = new FollowupReaction(reaction, followup);
            }

            return reaction;
        }

        /// <inheritdoc/>
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

        public string DebugRecipes(ISituationState situationState, IGameState gameState = null)
        {
            if (gameState == null)
            {
                gameState = GameStateProvider.Current;
            }

            if (situationState.State == StateEnum.Unstarted)
            {
                return (this.StartingRecipe != null) ? "Starting Recipe" : "No starting recipe.";
            }

            if (situationState.CurrentRecipe == null)
            {
                return "Situation has no current recipe, no recipes can target it.";
            }

            if (this.OngoingRecipes != null && this.GetOngoingRecipes().TryGetValue(situationState.CurrentRecipe, out var recipe))
            {
                return $"Ongoing Recipe: {situationState.CurrentRecipe}";
            }

            var sb = new StringBuilder();
            foreach (var conditional in this.GetConditionalRecipes())
            {
                var asConfig = conditional as ConditionalRecipeSolutionConfig;
                var canExecute = ConditionResult.Trace(() => conditional.IsConditionMet(gameState));
                sb.AppendLine($"Conditional Recipe: {asConfig?.Name ?? "Unknown"} ({canExecute})");
                if (!canExecute)
                {
                    sb.AppendLine($"- {canExecute.ToString()}");
                }
            }

            return sb.ToString();
        }

        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (string.IsNullOrEmpty(this.GetSituationId()))
            {
                throw new InvalidConfigException($"Operation {this.Name} must have a situation.");
            }
        }

        public override string ToString()
        {
            return $"OperationConfig(Name = {this.Name}, File = {FilesystemHelpers.GetRelativePath(this.FilePath, Autoccultist.AssemblyDirectory)}, Situation = {this.Situation})";
        }

        protected virtual string GetSituationId()
        {
            return this.Situation;
        }

        protected virtual IRecipeSolution GetStartingRecipe()
        {
            return this.StartingRecipe;
        }

        protected virtual IReadOnlyDictionary<string, IRecipeSolution> GetOngoingRecipes()
        {
            if (this.OngoingRecipes == null)
            {
                return new Dictionary<string, IRecipeSolution>();
            }

            return this.OngoingRecipes.ToDictionary(x => x.Key, x => (IRecipeSolution)x.Value);
        }

        protected virtual IReadOnlyList<IConditionalRecipeSolution> GetConditionalRecipes()
        {
            if (this.ConditionalRecipes == null)
            {
                return new List<IConditionalRecipeSolution>();
            }

            return this.ConditionalRecipes.Cast<IConditionalRecipeSolution>().ToList();
        }
    }
}
