namespace AutoccultistNS.GameState.Impl
{
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.UI;

    /// <summary>
    /// Implements the state of a situation from a situation controller.
    /// </summary>
    internal class SituationStateImpl : GameStateObject, ISituationState
    {
        private readonly string situationId;
        private readonly StateEnum state;
        private readonly bool isOccupied;
        private readonly string currentRecipe;
        private readonly float? recipeTimeRemaining;
        private readonly IReadOnlyCollection<ISituationSlot> recipeSlots;
        private readonly IReadOnlyCollection<ICardState> storedCards;
        private readonly IReadOnlyCollection<ICardState> outputCards;

        /// <summary>
        /// Initializes a new instance of the <see cref="SituationStateImpl"/> class.
        /// </summary>
        /// <param name="situation">The situation to represent the state of.</param>
        public SituationStateImpl(Situation situation)
        {
            // Precalculate all state data, as we represent a snapshot and do not want the state to change from under us.
            this.situationId = situation.VerbId;
            this.state = situation.State.Identifier;
            this.isOccupied = this.state != StateEnum.Unstarted;

            // Do not be confused by Situation.CurrentRecipe, as that shows shows the 'next' recipe for the current slots once the warmup is done.
            // Effectively, it shows the alt recipe that will be followed.
            // Our idea of the current recipe is what we are currently working on, not what the next will be.

            if (this.state == StateEnum.Ongoing)
            {
                // We want to know the recipe currently being processed, not the one that will trigger if the warmup completes.
                this.currentRecipe = situation.FallbackRecipeId;
                this.recipeTimeRemaining = situation.TimeRemaining;
            }
            else if (this.state == StateEnum.Complete)
            {
                // Recipe is complete, we want to know the thing we chose.
                this.currentRecipe = situation.CurrentRecipeId;
                this.recipeTimeRemaining = situation.TimeRemaining;
            }
            else
            {
                this.currentRecipe = null;
                this.recipeTimeRemaining = null;
            }

            var slots =
                from sphere in situation.GetSpheresByCategory(SphereCategory.Threshold)
                select new SituationSlotImpl(sphere);

            // We can create new ICardState states here, as IGameState only creates states for tabled cards, of which these are not.
            var stored =
                from sphere in situation.GetSpheresByCategory(SphereCategory.SituationStorage)
                from stack in sphere.GetElementStacks()
                from card in CardStateImpl.CardStatesFromStack(stack, CardLocation.Stored)
                select card;

            // Consider output stacks to be tabletop, as they are immediately grabbable.
            var output =
                from spheres in situation.GetSpheresByCategory(SphereCategory.Output)
                from stack in spheres.GetElementStacks()
                from card in CardStateImpl.CardStatesFromStack(stack, CardLocation.Tabletop)
                select card;

            this.recipeSlots = slots.ToArray();
            this.storedCards = stored.ToArray();
            this.outputCards = output.ToArray();
        }

        /// <inheritdoc/>
        public StateEnum State
        {
            get
            {
                this.VerifyAccess();
                return this.state;
            }
        }

        /// <inheritdoc/>
        public string SituationId
        {
            get
            {
                this.VerifyAccess();
                return this.situationId;
            }
        }

        /// <inheritdoc/>
        public bool IsOccupied
        {
            get
            {
                this.VerifyAccess();
                return this.isOccupied;
            }
        }

        /// <inheritdoc/>
        public string CurrentRecipe
        {
            get
            {
                this.VerifyAccess();
                return this.currentRecipe;
            }
        }

        public string CurrentRecipePortal
        {
            get
            {
                this.VerifyAccess();
                var recipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(this.currentRecipe);
                if (recipe == null)
                {
                    return null;
                }

                // Looks like the game is setting up to support multiple of these.
                // Should make this system be able to target any one of them.
                return recipe.PortalEffect;
            }
        }

        /// <inheritdoc/>
        public float? RecipeTimeRemaining
        {
            get
            {
                this.VerifyAccess();
                return this.recipeTimeRemaining;
            }
        }

        // <inheritdoc/>
        public IReadOnlyCollection<ISituationSlot> RecipeSlots
        {
            get
            {
                this.VerifyAccess();
                return this.recipeSlots;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ICardState> StoredCards
        {
            get
            {
                this.VerifyAccess();
                return this.storedCards;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ICardState> OutputCards
        {
            get
            {
                this.VerifyAccess();
                return this.outputCards;
            }
        }
    }
}
