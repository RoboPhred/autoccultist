namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using SecretHistories.Enums;
    using YamlDotNet.Core;

    public class OperationConfig : NamedConfigObject, IOperation, IReactor
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
            return $"OperationConfig(Name = {this.Name}, Situation = {this.Situation})";
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
