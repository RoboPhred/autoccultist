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
        private const int LifetimeHashLatchSeconds = 1;

        private readonly Lazy<ElementStack> consumed;

        private readonly string elementId;
        private readonly float lifetimeRemaining;
        private readonly bool isUnique;
        private readonly CardLocation location;
        private readonly bool isSlottable;
        private readonly IReadOnlyDictionary<string, int> aspects;
        private readonly string signature;
        private readonly Lazy<int> hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardStateImpl"/> class.
        /// </summary>
        /// <param name="sourceStack">The source stack to represent a card of.</param>
        /// <param name="location">The location of the card.</param>
        public CardStateImpl(ElementStack sourceStack, CardLocation location)
        {
            this.consumed = new Lazy<ElementStack>(() => CardStateImpl.TakeOneFromStack(sourceStack));

            this.elementId = sourceStack.EntityId;
            this.lifetimeRemaining = sourceStack.LifetimeRemaining;
            this.isUnique = sourceStack.Unique;

            var calcAspects = new AspectsDictionary();

            // We want the aspects of an individual card, not the sum total of the entire stack
            // GetAspects() will do this, but then also multiply the result by the stack quantity
            // Note: The game seems inclined to cache this data.  Is this a performance problem?
            calcAspects.CombineAspects(sourceStack.Element.Aspects);

            // Note: As far as I can tell, it is expected that any stack that has mutations will have a quantity of 1.
            // Mutated cards don't stack.
            calcAspects.ApplyMutations(sourceStack.Mutations);

            // This is included in the GetAspects() logic (with its default includeSelf true), and we relied on it in the past.
            calcAspects[this.elementId] = 1;
            this.aspects = calcAspects;

            this.signature = sourceStack.GetSignature();

            this.location = location;

            // I have no idea which of these conditions are actually necessary.
            var enRoute = sourceStack.Token.Sphere is EnRouteSphere;
            var tokenState = sourceStack.Token.CurrentState;

            // isSlottable should be true if we can yoink this card, even if it is not on the tabletop.
            // This allows a SlotCardAction to concievably target cards in verbs that are still in their warmup period.
            // This is useful in several "pro" strategies where sulochana is used in the talk verb to pauase the timer on influences.
            // Note: In practice, SlottableCardChooser still looks for tabletop only.
            // WARN: tokenState.InPlayerDrivenMotion and tokenState.InSystemDrivenMotion are unreliable, token
            //  states tend to randomly not be updated. See: UnknownState cards sitting on the board.
            this.isSlottable = !enRoute
                && !sourceStack.Defunct
                && !tokenState.InPlayerDrivenMotion()
                && !tokenState.InSystemDrivenMotion()
                && sourceStack.Token.Sphere.IsExteriorSphere;

            this.hashCode = new Lazy<int>(() => HashUtils.Hash(
                // Signature makes up for both elementId and aspects
                this.signature,
                // This changes every frame and is a bastard of a cache buster.
                // Round it to the nearest second, since we don't support fractional lifetime comparisons
                (int)Math.Round(this.lifetimeRemaining),
                this.isUnique,
                this.location,
                this.isSlottable));
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
        /// <returns>An enumerable of card states representing cards in the given stack.</returns>
        public static IEnumerable<CardStateImpl> CardStatesFromStack(ElementStack stack, CardLocation location)
        {
            for (var i = 0; i < stack.Quantity; i++)
            {
                yield return new CardStateImpl(stack, location);
            }
        }

        public override int GetHashCode()
        {
            return this.hashCode.Value;
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
    }
}
