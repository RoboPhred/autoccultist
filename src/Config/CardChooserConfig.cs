namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS;
    using AutoccultistNS.Config.Conditions;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    /// <summary>
    /// Represents a choice of a card based on various attributes.
    /// </summary>
    public class CardChooserConfig : NamedConfigObject, ICardChooser
    {
        /// <summary>
        /// Specify whether the card choice should go for the oldest or youngest card it can find.
        /// </summary>
        public enum CardAgeSelection
        {
            /// <summary>
            /// The choice should choose the card with the least lifetime remaining.
            /// </summary>
            Oldest,

            /// <summary>
            /// The choice should choose the card with the most lifetime remaining.
            /// </summary>
            Youngest,
        }

        /// <summary>
        /// Specifies whether the card choice should go for the card with the highest or lowest aspect weight.
        /// </summary>
        public enum CardAspectWeightSelection
        {
            /// <summary>
            /// The choice should choose the card with the most total aspect weight.
            /// </summary>
            Highest,

            /// <summary>
            /// The choice should choose the card with the least total aspect weight.
            /// </summary>
            Lowest,
        }

        /// <summary>
        /// Gets or sets the element id of the card to choose.
        /// If left empty, the element id will not be factored into the card choice.
        /// </summary>
        public string ElementId { get; set; }

        /// <summary>
        /// Gets or sets the location the card must be at to be chosen.
        /// </summary>
        public CardLocation? Location { get; set; }

        /// <summary>
        /// Gets or sets the list of element ids from which to choose a card.
        /// </summary>
        public List<string> AllowedElementIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the card must or must not be unique.
        /// </summary>
        public bool? Unique { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of aspect names to degrees to filter the cards by.
        /// If set, a matching card must have all of the specified aspects of at least the given degree.
        /// </summary>
        public Dictionary<string, ValueCondition> Aspects { get; set; }

        /// <summary>
        /// Gets or sets a list of aspects forbidden to be on the chosen card.
        /// Mainly used when specifying matching aspects.
        /// </summary>
        public List<string> ForbiddenAspects { get; set; }

        /// <summary>
        /// Gets or sets the aspect weight bias by which to choose cards.
        /// </summary>
        public CardAspectWeightSelection AspectWeightBias { get; set; }

        /// <summary>
        /// Gets or sets a list of elements forbidden from being matched.
        /// Mainly used when specifying matching aspects.
        /// </summary>
        public List<string> ForbiddenElementIds { get; set; }

        /// <summary>
        /// Gets or sets the age bias by which to choose time-limited cards.
        /// </summary>
        public CardAgeSelection? AgeBias { get; set; }

        /// <summary>
        /// Gets or sets a condition for the decay timer.
        /// </summary>
        public ValueCondition LifetimeRemaining { get; set; }

        /// <summary>
        /// Gets or sets a list of additional card choosers to filter this chooser by.
        /// </summary>
        public List<CardChooserConfig> From { get; set; }

        /// <summary>
        /// Choose a card from the given card states based on this filter's rules.
        /// </summary>
        /// <param name="cards">The cards to choose from.</param>
        /// <returns>The chosen card, or <c>null</c> if none were chosen.</returns>
        public ICardState ChooseCard(IEnumerable<ICardState> cards)
        {
            var candidates = this.FilterCards(cards);

            // Sort for age bias.
            if (this.AgeBias == CardAgeSelection.Oldest)
            {
                candidates = candidates.OrderBy(card => card.LifetimeRemaining);
            }
            else if (this.AgeBias == CardAgeSelection.Youngest)
            {
                candidates = candidates.OrderByDescending(card => card.LifetimeRemaining);
            }
            else if (this.AspectWeightBias == CardAspectWeightSelection.Highest)
            {
                candidates = candidates.OrderByDescending(card => this.GetSortWeight(card));
            }
            else if (this.AspectWeightBias == CardAspectWeightSelection.Lowest)
            {
                candidates = candidates.OrderBy(card => this.GetSortWeight(card));
            }
            else
            {
                // Don't care about aspects, so optimize for least important card across all aspects.
                candidates = candidates.OrderBy(card => card.Aspects.GetWeight());
            }

            return candidates.FirstOrDefault();
        }

        public override string ToString()
        {
            var content = new List<string>();

            content.Add($"Name: \"{this.Name}\"");

            if (this.ElementId != null)
            {
                content.Add($"elementId: {this.ElementId}");
            }

            if (this.AllowedElementIds != null && this.AllowedElementIds.Count > 0)
            {
                content.Add($"allowedElementIds: {string.Join(", ", this.AllowedElementIds)}");
            }

            if (this.ForbiddenElementIds != null && this.ForbiddenElementIds.Count > 0)
            {
                content.Add($"forbiddenElementIds: {string.Join(", ", this.ForbiddenElementIds)}");
            }

            if (this.Location.HasValue)
            {
                content.Add($"location: {this.Location}");
            }

            if (this.Aspects != null && this.Aspects.Count > 0)
            {
                content.Add($"aspects: {string.Join(", ", this.Aspects.Select(entry => $"{entry.Key}: {entry.Value}"))}");
            }

            if (this.ForbiddenAspects != null && this.ForbiddenAspects.Count > 0)
            {
                content.Add($"forbiddenAspects: {string.Join(", ", this.ForbiddenAspects)}");
            }

            if (this.Unique.HasValue)
            {
                content.Add($"unique: {this.Unique}");
            }

            if (this.LifetimeRemaining != null)
            {
                content.Add($"lifetimeRemaining: {this.LifetimeRemaining}");
            }

            if (this.AgeBias.HasValue)
            {
                content.Add($"ageBias: {this.AgeBias}");
            }

            return $"CardChooserConfig({string.Join(", ", content)})";
        }

        /// <inheritdoc/>
        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            if (string.IsNullOrEmpty(this.ElementId) && (this.Aspects == null || this.Aspects.Count == 0) && (this.AllowedElementIds == null || this.AllowedElementIds.Count == 0))
            {
                throw new InvalidConfigException("Card choice must have an elementId, allowedElementIds, or aspects.");
            }
        }

        protected virtual IEnumerable<ICardState> FilterCards(IEnumerable<ICardState> cards)
        {
            if (this.From != null && this.From.Count > 0)
            {
                cards = new HashSet<ICardState>(this.From.Select(x => x.ChooseCard(cards)));
            }

            // Once again, the lack of covariance in IReadOnlyDictionary comes back to bite us
            var aspectsAsCondition = this.Aspects?.ToDictionary(entry => entry.Key, entry => entry.Value as IValueCondition);

            return
                from card in cards
                where this.ElementId == null || card.ElementId == this.ElementId
                where this.Location == null || card.Location == this.Location
                where this.LifetimeRemaining?.IsConditionMet(card.LifetimeRemaining) != false
                where this.AllowedElementIds?.Contains(card.ElementId) != false
                where this.ForbiddenElementIds?.Contains(card.ElementId) != true
                where aspectsAsCondition == null || card.Aspects.HasAspects(aspectsAsCondition)
                where this.ForbiddenAspects?.Intersect(card.Aspects.Keys).Any() != true
                where !this.Unique.HasValue || card.IsUnique == this.Unique.Value
                where this.AdditionalFilter(card)
                select card;
        }

        /// <summary>
        /// Performs additional filtering on a chosen card.
        /// </summary>
        /// <param name="card">The card to filter.</param>
        /// <returns><c>true</c> if this card should be selected, or <c>false</c> if it should not.</returns>
        protected virtual bool AdditionalFilter(ICardState card)
        {
            return true;
        }

        private double GetSortWeight(ICardState card)
        {
            if (this.Aspects == null)
            {
                return card.Aspects.GetWeight();
            }
            else
            {
                var sortAspects = this.Aspects.ToDictionary(entry => entry.Key, entry => card.Aspects.ValueOrDefault(entry.Key));
                return sortAspects.GetWeight();
            }
        }
    }
}
