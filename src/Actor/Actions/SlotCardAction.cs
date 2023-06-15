namespace AutoccultistNS.Actor.Actions
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Tasks;
    using SecretHistories.Enums;

    // TODO: This should take a specific card reservation, not a card matcher.
    /// <summary>
    /// An action to slot a card into a slot of a situation.
    /// </summary>
    public class SlotCardAction : ActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlotCardAction"/> class.
        /// </summary>
        /// <param name="situationId">The situation id of the situation to slot the card into.</param>
        /// <param name="slotId">The slot id of the slot in the situation to slot the card into.</param>
        /// <param name="cardMatcher">The card matcher to choose a card to slot.</param>
        public SlotCardAction(string situationId, string slotId, ISlotCardChooser cardMatcher)
        {
            this.SituationId = situationId;
            this.SlotId = slotId;
            this.CardMatcher = cardMatcher;
        }

        public static bool UseItinerary { get; set; } = true;

        /// <summary>
        /// Gets the situation id of the situation to slot the card into.
        /// </summary>
        public string SituationId { get; }

        /// <summary>
        /// Gets the slot id of the slot in the situation to slot the card into.
        /// </summary>
        public string SlotId { get; }

        /// <summary>
        /// Gets the card matcher that will get the card to slot.
        /// </summary>
        public ISlotCardChooser CardMatcher { get; }

        public override string ToString()
        {
            return $"SlotCardAction(SituationId = {this.SituationId}, SlotId = {this.SlotId})";
        }

        /// <inheritdoc/>
        protected override async Task<bool> OnExecute(CancellationToken cancellationToken)
        {
            if (GameAPI.IsInMansus)
            {
                throw new ActionFailureException(this, "Cannot interact with situations when in the mansus.");
            }

            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                throw new ActionFailureException(this, "Situation is not available.");
            }

            var card = CardManager.ChooseCard(this.CardMatcher);
            if (card == null)
            {
                if (this.CardMatcher.Optional)
                {
                    return false;
                }

                throw new ActionFailureException(this, $"No matching card was found for situation {this.SituationId} slot {this.SlotId}.");
            }

            var sphere = situation.GetCurrentThresholdSpheres().FirstOrDefault(x => x.GoverningSphereSpec.Id == this.SlotId);
            if (sphere == null)
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} has no matching slot {this.SlotId}.");
            }

            var stack = card.ToElementStack();

            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
            {
                var itinerary = sphere.GetItineraryFor(stack.Token);

                var itineraryDuration = (float)AutoccultistSettings.ActionDelay.TotalSeconds * 2 / 3;
                var postDelayDuration = AutoccultistSettings.ActionDelay - TimeSpan.FromSeconds(itineraryDuration);

                itinerary.WithDuration(itineraryDuration).Depart(stack.Token, new Context(Context.ActionSource.DoubleClickSend));

                GameStateProvider.Invalidate();

                // Would be nice if there was a way to subscribe to the itinerary, but whatever...
                var awaitSphereFilled = new AwaitConditionTask(() => sphere.GetTokens().Contains(stack.Token), cancellationToken);
                if (await Task.WhenAny(awaitSphereFilled.Task, Task.Delay(1000, cancellationToken)) != awaitSphereFilled.Task)
                {
                    throw new ActionFailureException(this, $"Timed out waiting for card {stack.Element.Id} to arrive in slot {this.SlotId} in situation {this.SituationId}.");
                }

                GameStateProvider.Invalidate();

                await Task.Delay(postDelayDuration, cancellationToken);
            }
            else
            {
                // Even though we passed CanAcceptToken above, we should be gentle and re-try.
                if (!sphere.TryAcceptToken(stack.Token, new Context(Context.ActionSource.DoubleClickSend)))
                {
                    throw new ActionFailureException(this, $"Slot {this.SlotId} on situation {this.SituationId} cannot accept card {card.ElementId}.");
                }

                GameStateProvider.Invalidate();
            }

            return true;
        }
    }
}
