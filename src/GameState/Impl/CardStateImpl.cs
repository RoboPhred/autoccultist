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
        private readonly Lazy<ElementStack> consumed;

        private readonly string elementId;
        private readonly float lifetimeRemaining;
        private readonly bool isUnique;
        private readonly CardLocation location;
        private readonly bool isSlottable;
        private readonly IReadOnlyDictionary<string, int> aspects;

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

            this.location = location;

            // I have no idea which of these conditions are actually necessary.
            var enRoute = sourceStack.Token.Sphere is EnRouteSphere;
            var tokenState = sourceStack.Token.CurrentState;

            // Do we really need to check if its on the tabletop?  We could slot cards sitting around unused inside verbs, but we probably only want to take from table.
            // Might revisit this if we ever support the technique of slotting a card to pause the timer.
            this.isSlottable = location == CardLocation.Tabletop && !enRoute && !tokenState.InPlayerDrivenMotion() && !tokenState.InSystemDrivenMotion() && !sourceStack.Defunct;
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

            return stack;
        }
    }
}
