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
        public CardAspectWeightSelection? AspectWeightBias { get; set; }

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

        /// <inheritdoc/>
        public IEnumerable<ICardState> SelectChoices(IEnumerable<ICardState> cards, IGameState state, CardChooserHints hints = CardChooserHints.None)
        {
            // We are often called with the same `cards` input, usually IGameState.GetAllCards or IGameState.TabletopCards.
            // We should make an effort to cache these.
            // Since these choosers are often used in the same context, this cache should be decently effective, especially for shared conditions
            // (such as has-slushfund-major)
            // Note: This is a good idea but breaking the bot somehow.
            // var hash = HashUtils.Hash(state, HashUtils.HashAllUnordered(cards));
            // var candidates = CacheUtils.Compute(this, nameof(this.SelectChoices), hash, () => this.FilterCards(cards, state).ToArray());
            var candidates = this.FilterCards(cards, state);

            if (hints.HasFlag(CardChooserHints.IgnorePriority))
            {
                return candidates;
            }

            return this.OrderCards(candidates, state);
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

        /// <summary>
        /// Filters the given cards by the conditions specified in this config.
        /// </summary>
        /// <param name="cards">The cards to filter.</param>
        /// <returns>The filtered cards.</returns>
        protected virtual IEnumerable<ICardState> FilterCards(IEnumerable<ICardState> cards, IGameState state)
        {
            if (this.From != null && this.From.Count > 0)
            {
                cards = new HashSet<ICardState>(this.From.Select(x => x.ChooseCard(cards, state)));
            }

            // Once again, the lack of covariance in IReadOnlyDictionary comes back to bite us
            var aspectsAsCondition = this.Aspects?.ToDictionary(entry => entry.Key, entry => entry.Value as IValueCondition);

            return
                from card in cards
                where this.ElementId == null || card.ElementId == this.ElementId
                where this.Location == null || card.Location == this.Location
                where this.LifetimeRemaining?.IsConditionMet(card.LifetimeRemaining, state) != false
                where this.AllowedElementIds?.Contains(card.ElementId) != false
                where this.ForbiddenElementIds?.Contains(card.ElementId) != true
                where aspectsAsCondition == null || card.Aspects.HasAspects(aspectsAsCondition, state)
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

        protected virtual IEnumerable<ICardState> OrderCards(IEnumerable<ICardState> cards, IGameState state)
        {
            // This is a bit silly, but we need it as we are unsure what the first ordering is going to be, and want to use ThenBy.
            var ordering = cards.OrderBy((_) => (int)0);

            // Sort for weight bias first, as it will have the most identical hits.
            if (this.AspectWeightBias == CardAspectWeightSelection.Highest)
            {
                ordering = ordering.ThenByDescending(card => this.GetSortWeight(card));
            }
            else if (this.AspectWeightBias == CardAspectWeightSelection.Lowest)
            {
                ordering = ordering.ThenBy(card => this.GetSortWeight(card));
            }

            // Then sort by desired age.
            if (this.AgeBias == CardAgeSelection.Oldest)
            {
                ordering = ordering.ThenBy(card => card.LifetimeRemaining);
            }
            else if (this.AgeBias == CardAgeSelection.Youngest)
            {
                ordering = ordering.ThenByDescending(card => card.LifetimeRemaining);
            }

            // Then sort by total weight.  This is still desirable even with AspectWeightBias, but only if
            // we had specific aspects we pre-sorted by
            if (!this.AspectWeightBias.HasValue || this.Aspects != null)
            {
                ordering = ordering.ThenBy(card => card.Aspects.GetWeight());
            }

            // Then by lifetime remaining, lowest first, but only if that wasnt already decided above.
            if (!this.AgeBias.HasValue)
            {
                ordering = ordering.ThenBy(card => card.LifetimeRemaining);
            }

            // Finally, sort by signature, so we get deterministic draws for the board state.
            ordering = ordering.ThenBy(card => card.Signature);

            return ordering;
        }

        private double GetSortWeight(ICardState card)
        {
            if (this.Aspects == null || this.Aspects.Count == 0)
            {
                return card.Aspects.GetWeight();
            }
            else
            {
                return card.Aspects.GetWeight(this.Aspects.Keys);
            }
        }
    }
}
