namespace AutoccultistNS.GameState.Impl
{
    using System;
    using System.Collections.Generic;
    using SecretHistories.Core;
    using SecretHistories.Spheres;
    using SecretHistories.UI;

    /// <summary>
    /// The backing implementation of the state of a card as derived from an element stack.
    /// </summary>
    internal class CardStateImpl : GameStateObject, ICardState
    {
        /// <summary>
        /// To avoid bogging down the condition logic, we limit how many cards we are willing to produce from a single stack.
        /// We can do this as each card in a stack is guarenteed to be identical to all other cards, so the only thing affected
        /// by this will be conditions that want to count that type of card.
        /// </summary>
        private const int CardsInStackLimit = 50;
        private const int LifetimeHashLatchSeconds = 1;

        private static HashSet<string> warnedCardLimitByElement = new();

        private readonly Lazy<ElementStack> consumed;

        private readonly string elementId;
        private readonly float lifetimeRemaining;
        private readonly bool isUnique;
        private readonly CardLocation location;
        private readonly string situation;
        private readonly bool isSlottable;
        private readonly IReadOnlyDictionary<string, int> aspects;
        private readonly string signature;
        private readonly int hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardStateImpl"/> class.
        /// </summary>
        /// <param name="sourceStack">The source stack to represent a card of.</param>
        /// <param name="location">The location of the card.</param>
        /// <param name="situation">The situation the card is in.</param>
        /// <param name="aspects">The precomputed aspects of the card.</param>
        /// <param name="slottable">Whether the card is slottable.</param>
        /// <param name="hashCode">The precomputed hash code of the card.</param>
        private CardStateImpl(ElementStack sourceStack, CardLocation location, string situation, IReadOnlyDictionary<string, int> aspects, bool slottable, int hashCode)
        {
            this.consumed = new Lazy<ElementStack>(() => CardStateImpl.TakeOneFromStack(sourceStack));

            this.elementId = sourceStack.EntityId;
            this.lifetimeRemaining = sourceStack.LifetimeRemaining;
            this.isUnique = sourceStack.Unique;

            this.aspects = aspects;

            this.signature = sourceStack.GetSignature();

            this.location = location;
            this.situation = situation;
            this.isSlottable = slottable;

            // Signature makes up for both elementId and aspects, so we dont need to include either in this.
            // Lifetime remaining changes every frame and is a bastard of a cache buster.
            this.hashCode = hashCode;
        }

        /// <inheritdoc/>
        public string ElementId
        {
            get
            {
                this.VerifyAccess();
                return this.elementId;
            }
        }

        /// <inheritdoc/>
        public float LifetimeRemaining
        {
            get
            {
                this.VerifyAccess();
                return this.lifetimeRemaining;
            }
        }

        /// <inheritdoc/>
        public bool IsUnique
        {
            get
            {
                this.VerifyAccess();
                return this.isUnique;
            }
        }

        /// <inheritdoc/>
        public CardLocation Location
        {
            get
            {
                this.VerifyAccess();
                return this.location;
            }
        }

        /// <inheritdoc/>
        public string Situation
        {
            get
            {
                this.VerifyAccess();
                return this.situation;
            }
        }

        /// <inheritdoc/>
        public bool IsSlottable
        {
            get
            {
                this.VerifyAccess();
                return this.isSlottable;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, int> Aspects
        {
            get
            {
                this.VerifyAccess();
                return this.aspects;
            }
        }

        /// <inheritdoc/>
        public string Signature
        {
            get
            {
                this.VerifyAccess();
                return this.signature;
            }
        }

        /// <summary>
        /// Gets an enumerable of card states for cards in the current stack.
        /// </summary>
        /// <param name="stack">The stack to derive card states from.</param>
        /// <param name="location">The location of the card stack.</param>
        /// <param name="situation">The situation the card stack is in.</param>
        /// <returns>An enumerable of card states representing cards in the given stack.</returns>
        public static IEnumerable<CardStateImpl> CardStatesFromStack(ElementStack stack, CardLocation location, string situation)
        {
            var count = stack.Quantity;
            if (count > CardsInStackLimit)
            {
                if (!warnedCardLimitByElement.Contains(stack.EntityId))
                {
                    warnedCardLimitByElement.Add(stack.EntityId);
                    Autoccultist.LogWarn($"Card limit exceeded for element {stack.EntityId}: {count} > {CardsInStackLimit}");
                    GameAPI.Notify("Autoccultist Warning", $"Card limit exceeded for element {stack.EntityId}: {count} > {CardsInStackLimit}.  Ignoring remaining cards.");
                }

                count = CardsInStackLimit;
            }

            var aspects = new AspectsDictionary();

            // We want the aspects of an individual card, not the sum total of the entire stack
            // GetAspects() will do this, but then also multiply the result by the stack quantity
            // Note: The game seems inclined to cache this data.  Is this a performance problem?
            aspects.CombineAspects(stack.Element.Aspects);

            // Note: As far as I can tell, it is expected that any stack that has mutations will have a quantity of 1.
            // Mutated cards don't stack.
            aspects.ApplyMutations(stack.Mutations);

            // This is included in the GetAspects() logic (with its default includeSelf true), and we relied on it in the past.
            aspects[stack.Element.Id] = 1;

            // isSlottable should be true if we can yoink this card, even if it is not on the tabletop.
            // This allows a SlotCardAction to concievably target cards in verbs that are still in their warmup period.
            // This is useful in several "pro" strategies where sulochana is used in the talk verb to pauase the timer on influences.
            // Note: In practice, SlottableCardChooser still looks for tabletop only.
            // WARN: tokenState.InPlayerDrivenMotion and tokenState.InSystemDrivenMotion are unreliable, token
            //  states tend to randomly not be updated. See: UnknownState cards sitting on the board.
            var enRoute = stack.Token.Sphere is EnRouteSphere;
            var tokenState = stack.Token.CurrentState;
            var slottable = !enRoute
                && !stack.Defunct
                && !tokenState.InPlayerDrivenMotion()
                && !tokenState.InSystemDrivenMotion()
                && stack.Token.Sphere.IsExteriorSphere;

            var hashCode = MakeHashCode(stack, location, slottable);

            // Hash codes will be the same for every entry, so we can save some time here.
            for (var i = 0; i < count; i++)
            {
                // Large stacks share the same values for all properties, so we can precompute most of this.
                yield return new CardStateImpl(stack, location, situation, aspects, slottable, hashCode);
            }
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        /// <inheritdoc/>
        public ElementStack ToElementStack()
        {
            this.VerifyAccess();
            return this.consumed.Value;
        }

        /// <summary>
        /// Takes a stack of a single card from an existing stack.
        /// </summary>
        /// <param name="stack">The stack to obtain a card from.</param>
        /// <returns>A stack of a single card.</returns>
        private static ElementStack TakeOneFromStack(ElementStack stack)
        {
            if (stack.Quantity > 1)
            {
                return stack.Token.CalveToken(1).Payload as ElementStack;
            }

            stack.Token.RequestHomeLocationFromCurrentSphere();
            return stack;
        }

        private static int MakeHashCode(ElementStack stack, CardLocation location, bool isSlottable)
        {
            return HashUtils.Hash(
                stack.GetSignature(),
                (int)Math.Round(stack.LifetimeRemaining),
                stack.Unique,
                location,
                isSlottable);
        }
    }
}
