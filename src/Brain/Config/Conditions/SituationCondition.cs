namespace Autoccultist.Brain.Config.Conditions
{
    using System.Collections.Generic;
    using System.Linq;
    using Autoccultist.GameState;

    /// <summary>
    /// A condition dealing with matching the state of situations.
    /// </summary>
    public class SituationCondition : IGameStateConditionConfig
    {
        /// <summary>
        /// Possible states to check situations against.
        /// </summary>
        public enum SituationStateConfig
        {
            /// <summary>
            /// The situation is not present on the table
            /// </summary>
            Missing,

            /// <summary>
            /// The situation is present, but not currently running a recipe.
            /// </summary>
            Idle,

            /// <summary>
            /// The situation is present and currently operating on a recipe.
            /// </summary>
            Ongoing,
        }

        /// <summary>
        /// Gets or sets the situation id for which all the other condition properties will target.
        /// </summary>
        public string Situation { get; set; }

        /// <summary>
        /// Gets or sets the state the situation is required to be in.
        /// If null, the state will not be checked.
        /// </summary>
        public SituationStateConfig? State { get; set; }

        /// <summary>
        /// Gets or sets the recipe the situation is required to be processing.
        /// If null, this condition will not be checked.
        /// </summary>
        public string Recipe { get; set; }

        /// <summary>
        /// Gets or sets a time comparison to the time remaining for the situation's current recipe.
        /// If null, this condition will not be checked.
        /// </summary>
        public TimeComparison TimeRemaining { get; set; }

        /// <summary>
        /// Gets or sets the card condition matching cards stored inside this condition, excluding slotted cards.
        /// If null, this condition will not be checked.
        /// </summary>
        public ICardConditionConfig StoredCardsMatch { get; set; }

        /// <summary>
        /// Gets or sets the card set condition matching cards slotted into the current ongoing slots of the situation.
        /// If null, this condition will not be checked.
        /// </summary>
        public ICardConditionConfig SlottedCardsMatch { get; set; }

        /// <summary>
        /// Gets or sets the card set condition matching all cards in play by this situation, including slotted and stored cards.
        /// If null, this condition will not be checked.
        /// </summary>
        public ICardConditionConfig ContainedCardsMatch { get; set; }

        /// <summary>
        /// Gets or sets a dictionary containing asset ids to degrees that must be satisfied in this situation.
        /// Aspects are taken from all contained cards, both slotted and stored.
        /// If null, this condition will not be checked.
        /// </summary>
        public Dictionary<string, int> ContainedAspects { get; set; }

        /// <inheritdoc/>
        public void Validate()
        {
            if (string.IsNullOrEmpty(this.Situation))
            {
                throw new InvalidConfigException("SituationCondition must have a situationId.");
            }

            this.TimeRemaining?.Validate();
        }

        /// <inheritdoc/>
        public bool IsConditionMet(IGameState state)
        {
            var situation = state.Situations.FirstOrDefault(x => x.SituationId == this.Situation);
            if (situation == null)
            {
                return this.State == SituationStateConfig.Missing;
            }

            if (this.Recipe != null && situation.CurrentRecipe != this.Recipe)
            {
                return false;
            }

            if (this.TimeRemaining != null && (!situation.IsOccupied || !this.TimeRemaining.IsComparisonTrue(situation.RecipeTimeRemaining ?? 0)))
            {
                return false;
            }

            if (this.State == SituationStateConfig.Idle || this.State == SituationStateConfig.Ongoing)
            {
                if (situation.IsOccupied != (this.State == SituationStateConfig.Ongoing))
                {
                    return false;
                }
            }

            if (this.StoredCardsMatch != null)
            {
                var cards = situation.StoredCards;
                if (!this.StoredCardsMatch.CardsMatchSet(cards))
                {
                    return false;
                }
            }

            if (this.SlottedCardsMatch != null)
            {
                var cards = situation.SlottedCards;
                if (!this.SlottedCardsMatch.CardsMatchSet(cards))
                {
                    return false;
                }
            }

            if (this.ContainedCardsMatch != null)
            {
                var cards = situation.StoredCards.Concat(situation.SlottedCards).ToList();
                if (!this.ContainedCardsMatch.CardsMatchSet(cards))
                {
                    return false;
                }
            }

            if (this.ContainedAspects != null)
            {
                var aspects = situation.GetAspects();
                if (!aspects.HasAspects(this.ContainedAspects))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
