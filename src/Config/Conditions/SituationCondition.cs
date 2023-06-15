namespace AutoccultistNS.Config.Conditions
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.GameState;
    using YamlDotNet.Core;

    /// <summary>
    /// A condition dealing with matching the state of situations.
    /// </summary>
    public class SituationCondition : ConditionConfig
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
        public ValueCondition TimeRemaining { get; set; }

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
        public Dictionary<string, ValueCondition> ContainsAspects { get; set; }

        /// <inheritdoc/>
        public override ConditionResult IsConditionMet(IGameState state)
        {
            var situation = state.Situations.FirstOrDefault(x => x.SituationId == this.Situation);
            if (situation == null)
            {
                if (this.State == SituationStateConfig.Missing)
                {
                    return ConditionResult.Success;
                }

                return new SituationConditionFailure(this.Situation, "is not missing");
            }

            if (this.Recipe != null && situation.CurrentRecipe != this.Recipe)
            {
                return new SituationConditionFailure(this.Situation, $"is not performing recipe {this.Recipe}");
            }

            if (this.TimeRemaining != null && (!situation.IsOccupied || !this.TimeRemaining.IsConditionMet(situation.RecipeTimeRemaining ?? 0)))
            {
                return new SituationConditionFailure(this.Situation, $"has {situation.RecipeTimeRemaining} time remaining, which does not match {this.TimeRemaining}");
            }

            if (this.State == SituationStateConfig.Idle || this.State == SituationStateConfig.Ongoing)
            {
                if (situation.IsOccupied != (this.State == SituationStateConfig.Ongoing))
                {
                    return new SituationConditionFailure(this.Situation, $"is {(situation.IsOccupied ? "not " : string.Empty)}ongoing");
                }
            }

            if (this.StoredCardsMatch != null)
            {
                var cards = situation.StoredCards;
                var matchResult = this.StoredCardsMatch.CardsMatchSet(cards);
                if (!matchResult)
                {
                    return new AddendedConditionFailure(matchResult, $"when looking at stored cards for situation {this.Situation}");
                }
            }

            var slottedCards = situation.GetSlottedCards().ToList();

            if (this.SlottedCardsMatch != null)
            {
                var matchResult = this.SlottedCardsMatch.CardsMatchSet(slottedCards);
                if (!matchResult)
                {
                    return new AddendedConditionFailure(matchResult, $"when looking at slotted cards for situation {this.Situation}");
                }
            }

            if (this.ContainedCardsMatch != null)
            {
                var cards = situation.StoredCards.Concat(slottedCards);
                var matchResult = this.ContainedCardsMatch.CardsMatchSet(cards);
                if (!matchResult)
                {
                    return new AddendedConditionFailure(matchResult, $"when looking at all cards for situation {this.Situation}");
                }
            }

            if (this.ContainsAspects != null)
            {
                var aspects = situation.GetAspects();

                // Damned lack of covariance on IReadOnlyDictionary
                var containedAspects = this.ContainsAspects.ToDictionary(entry => entry.Key, entry => (IValueCondition)entry.Value);

                if (!aspects.HasAspects(containedAspects))
                {
                    // TODO: ConditionFailure for HasAspects / IValueCondition
                    return new SituationConditionFailure(this.Situation, $"does not match required aspect conditions {string.Join(", ", this.ContainsAspects.Keys)}");
                }
            }

            return ConditionResult.Success;
        }

        /// <inheritdoc/>
        protected override void OnAfterDeserialized(Mark start, Mark end)
        {
            if (string.IsNullOrEmpty(this.Situation))
            {
                throw new InvalidConfigException("SituationCondition must have a situationId.");
            }

            base.OnAfterDeserialized(start, end);
        }
    }
}
