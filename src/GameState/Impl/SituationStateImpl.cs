namespace Autoccultist.GameState.Impl
{
    using System.Collections.Generic;
    using System.Linq;
    using Assets.TabletopUi;

    /// <summary>
    /// Implements the state of a situation from a situation controller.
    /// </summary>
    internal class SituationStateImpl : GameStateObject, ISituationState
    {
        private readonly string situationId;
        private readonly bool isOccupied;
        private readonly string currentRecipe;
        private readonly float? recipeTimeRemaining;
        private readonly IReadOnlyCollection<ICardState> storedCards;
        private readonly IReadOnlyCollection<ICardState> slottedCards;

        /// <summary>
        /// Initializes a new instance of the <see cref="SituationStateImpl"/> class.
        /// </summary>
        /// <param name="situation">The situation to represent the state of.</param>
        public SituationStateImpl(SituationController situation)
        {
            // Precalculate all state data, as we represent a snapshot and do not want the state to change from under us.
            this.situationId = situation.GetTokenId();
            this.isOccupied = situation.SituationClock.State != SituationState.Unstarted;

            var clock = situation.SituationClock;
            if (clock.State == SituationState.FreshlyStarted || clock.State == SituationState.Ongoing)
            {
                this.currentRecipe = clock.RecipeId;
                this.recipeTimeRemaining = clock.TimeRemaining;
            }
            else
            {
                this.currentRecipe = null;
                this.recipeTimeRemaining = null;
            }

            // We can create new ICardState states here, as IGameState only creates states for tabled cards, of which these are not.
            var stored =
                from stack in situation.situationWindow.GetStoredStacks()
                from card in CardStateImpl.CardStatesFromStack(stack)
                select card;
            var slotted =
                from stack in situation.situationWindow.GetOngoingSlots().Select(x => x.GetElementStackInSlot())
                where stack != null
                from card in CardStateImpl.CardStatesFromStack(stack)
                select card;

            this.storedCards = stored.ToArray();
            this.slottedCards = slotted.ToArray();
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

        /// <inheritdoc/>
        public float? RecipeTimeRemaining
        {
            get
            {
                this.VerifyAccess();
                return this.recipeTimeRemaining;
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
        public IReadOnlyCollection<ICardState> SlottedCards
        {
            get
            {
                this.VerifyAccess();
                return this.slottedCards;
            }
        }
    }
}
