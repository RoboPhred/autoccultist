namespace AutoccultistNS.Actor.Actions
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Tasks;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Spheres;

    public class ExecuteRecipeAction : ActionBase
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

        public ExecuteRecipeAction(string situationId, IRecipeSolution recipeSolution, string recipeName)
        {
            this.SituationId = situationId;
            this.RecipeSolution = recipeSolution;
            this.RecipeName = recipeName;
        }

        public string SituationId { get; }

        public string RecipeName { get; }

        public IRecipeSolution RecipeSolution { get; }

        public override string ToString()
        {
            return $"ExecuteRecipeAction(RecipeName = {this.RecipeName}, SituationId = {this.SituationId})";
        }

        protected override async Task<bool> OnExecute(CancellationToken cancellationToken)
        {
            if (GameAPI.IsInMansus)
            {
                throw new ActionFailureException(this, "Cannot interact with situations when in the mansus.");
            }

            if (this.RecipeSolution.SlotSolutions == null || this.RecipeSolution.SlotSolutions.Count == 0)
            {
                // Nothing to do.
                // This is probably a mansus-only recipe.
                return false;
            }

            await this.FillSlots(cancellationToken);

            return true;
        }

        private async Task FillSlots(CancellationToken cancellationToken)
        {
            var situation = this.GetSituation();

            var firstSlot = situation.GetCurrentThresholdSpheres().FirstOrDefault();
            if (!firstSlot)
            {
                // There are ongoing recipes without slots.
                // ...we might try to execute recipe solutions for them if these solutions have side effects like mansus interactions
                // or orphaning the operation.
                return;
            }

            // TODO: Use ICardChooserExtensions.ChooseAll to find a solution to all the card matchers that allows for choosing lower
            // priority cards if there are conflicts.
            if (!await this.FillSlot(firstSlot, cancellationToken))
            {
                throw new ActionFailureException(this, $"Failed to fill first slot of recipe {this.RecipeName} in situation {this.SituationId}.");
            }

            var remainingSlots = situation.GetCurrentThresholdSpheres().Where(x => x.Id != firstSlot.Id).ToArray();
            foreach (var slot in remainingSlots)
            {
                await this.FillSlot(slot, cancellationToken);
            }
        }

        private async Task<bool> FillSlot(Sphere slotSphere, CancellationToken cancellationToken)
        {
            var slotId = slotSphere.GoverningSphereSpec.Id;
            if (!this.RecipeSolution.SlotSolutions.TryGetValue(slotId, out var cardMatcher))
            {
                return false;
            }

            var card = cardMatcher.ChooseCard(GameStateProvider.Current.TabletopCards, GameStateProvider.Current);
            if (card == null)
            {
                if (cardMatcher.Optional)
                {
                    return false;
                }

                throw new ActionFailureException(this, $"No matching card was found for situation {this.SituationId} recipe {this.RecipeName} slot {slotId}.");
            }

            var stack = card.ToElementStack();

            if (!slotSphere.CanAcceptToken(stack.Token))
            {
                throw new ActionFailureException(this, $"Recipe {this.RecipeName} slot {slotId} on situation {this.SituationId} cannot accept card {stack.Element.Id}.");
            }

            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
            {
                var itinerary = slotSphere.GetItineraryFor(stack.Token);

                itinerary.WithDuration(0.1f).Depart(stack.Token, new Context(Context.ActionSource.DoubleClickSend));

                try
                {
                    // Would be nice if there was a way to subscribe to the itinerary, but whatever...
                    await RealtimeDelay.Timeout((c) => AwaitConditionTask.From(() => slotSphere.GetTokens().Contains(stack.Token), c), Timeout, cancellationToken);
                }
                catch (TimeoutException)
                {
                    throw new ActionFailureException(this, $"Timed out waiting for card {stack.Element.Id} to arrive in slot {slotId} in situation {this.SituationId}.  Our token ended up in state {stack.Token.CurrentState} at sphere {stack.Token.Sphere.Id}.");
                }

                // Wait the full delay after slotting the card.
                await MechanicalHeart.AwaitBeat(cancellationToken, AutoccultistSettings.ActionDelay);
            }
            else
            {
                // Even though we passed CanAcceptToken above, we should be gentle and re-try.
                if (!slotSphere.TryAcceptToken(stack.Token, new Context(Context.ActionSource.DoubleClickSend)))
                {
                    throw new ActionFailureException(this, $"Recipe {this.RecipeName} slot {slotId} on situation {this.SituationId} cannot accept card {card.ElementId}.");
                }

                GameStateProvider.Invalidate();
            }

            // Hold us for the next beat, if we are not running live.
            await MechanicalHeart.AwaitBeatIfStopped(cancellationToken);

            return true;
        }

        private Situation GetSituation()
        {
            var situation = GameAPI.GetSituation(this.SituationId);
            if (situation == null)
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} not found.");
            }

            return situation;
        }
    }
}
