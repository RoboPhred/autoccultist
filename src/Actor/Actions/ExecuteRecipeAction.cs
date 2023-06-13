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
        public string SituationId { get; }

        public string RecipeName { get; }

        public IRecipeSolution RecipeSolution { get; }

        public bool LeaveSituationOpen { get; }

        public ExecuteRecipeAction(string situationId, IRecipeSolution recipeSolution, string recipeName, bool leaveOpen = false)
        {
            this.SituationId = situationId;
            this.RecipeSolution = recipeSolution;
            this.RecipeName = recipeName;
            this.LeaveSituationOpen = leaveOpen;
        }

        protected override async Task<ActionResult> OnExecute(CancellationToken cancellationToken)
        {
            if (GameAPI.IsInMansus)
            {
                throw new ActionFailureException(this, "Cannot interact with situations when in the mansus.");
            }

            await this.PrepareSituation(cancellationToken);

            await this.FillSlots(cancellationToken);

            await this.FinalizeSituation(cancellationToken);

            return ActionResult.Completed;
        }

        private async Task PrepareSituation(CancellationToken cancellationToken)
        {
            var situation = this.GetSituation();

            if (!situation.IsOpen)
            {
                situation.OpenAt(situation.Token.Location);
                await Task.Delay(AutoccultistSettings.ActionDelay, cancellationToken);
            }

            if (situation.State.Identifier == StateEnum.Unstarted)
            {
                if (situation.GetSpheresByCategory(SphereCategory.Threshold).Any(s => s.GetTokens().Any()))
                {
                    situation.DumpUnstartedBusiness();
                    await Task.Delay(AutoccultistSettings.ActionDelay, cancellationToken);
                }
            }
            else if (situation.State.Identifier == StateEnum.Complete)
            {
                situation.Conclude();
                await Task.Delay(AutoccultistSettings.ActionDelay, cancellationToken);
            }
        }

        private async Task FillSlots(CancellationToken cancellationToken)
        {
            var situation = this.GetSituation();

            var firstSlot = situation.GetSpheresByCategory(SphereCategory.Threshold).FirstOrDefault();
            if (!firstSlot)
            {
                throw new ActionFailureException(this, $"Situation {this.SituationId} has no slots.");
            }

            if (!await this.FillSlot(firstSlot, cancellationToken))
            {
                throw new ActionFailureException(this, $"Failed to fill first slot of recipe {this.RecipeName} in situation {this.SituationId}.");
            }

            var remainingSlots = situation.GetSpheresByCategory(SphereCategory.Threshold).Where(x => x.Id != firstSlot.Id).ToArray();
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

            var card = CardManager.ChooseCard(cardMatcher);
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
                throw new ActionFailureException(this, $"Recipe {this.RecipeName} slot {slotId} on situation {this.SituationId} cannot accept card {card.ElementId}.");
            }

            if (AutoccultistSettings.ActionDelay > TimeSpan.Zero)
            {
                var itinerary = slotSphere.GetItineraryFor(stack.Token);

                var itineraryDuration = (float)AutoccultistSettings.ActionDelay.TotalSeconds * 2 / 3;
                var postDelayDuration = AutoccultistSettings.ActionDelay - TimeSpan.FromSeconds(itineraryDuration);

                itinerary.WithDuration(itineraryDuration).Depart(stack.Token, new Context(Context.ActionSource.DoubleClickSend));

                GameStateProvider.Invalidate();

                // Would be nice if there was a way to subscribe to the itinerary, but whatever...
                var awaitSphereFilled = new AwaitConditionTask(() => slotSphere.GetTokens().Contains(stack.Token), cancellationToken);
                if (await Task.WhenAny(awaitSphereFilled.Task, Task.Delay(1000, cancellationToken)) != awaitSphereFilled.Task)
                {
                    throw new ActionFailureException(this, $"Timed out waiting for card to arrive in slot {slotId} in situation {this.SituationId}.");
                }

                GameStateProvider.Invalidate();

                await Task.Delay(postDelayDuration, cancellationToken);
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

            return true;
        }

        private async Task FinalizeSituation(CancellationToken cancellationToken)
        {
            var situation = this.GetSituation();

            var doClose = !this.LeaveSituationOpen && situation.IsOpen;

            if (situation.State.Identifier == StateEnum.Unstarted)
            {
                situation.TryStart();
                if (situation.State.Identifier == StateEnum.Unstarted)
                {
                    throw new ActionFailureException(this, $"Failed to start situation {this.SituationId} for recipe.");
                }

                GameStateProvider.Invalidate();

                if (doClose)
                {
                    await Task.Delay(AutoccultistSettings.ActionDelay, cancellationToken);
                }
            }

            if (doClose)
            {
                situation.Close();
            }
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