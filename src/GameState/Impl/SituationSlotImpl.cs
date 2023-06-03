namespace AutoccultistNS.GameState.Impl
{
    using System.Linq;
    using SecretHistories.Spheres;

    internal class SituationSlotImpl : GameStateObject, ISituationSlot
    {
        private Sphere sphere;
        private ICardState card;

        /// <summary>
        /// Initializes a new instance of the <see cref="SituationSlotImpl"/> class.
        /// </summary>
        /// <param name="sphere">The sphere to represent.</param>
        public SituationSlotImpl(Sphere sphere)
        {
            this.sphere = sphere;

            var elementStack = sphere.GetElementStacks().FirstOrDefault();
            if (elementStack != null)
            {
                this.card = CardStateImpl.CardStatesFromStack(elementStack, CardLocation.Slotted).First();
            }
            else
            {
                this.card = null;
            }
        }

        public string SpecId
        {
            get
            {
                this.VerifyAccess();
                return this.sphere.GoverningSphereSpec.Id;
            }
        }

        public ICardState Card
        {
            get
            {
                this.VerifyAccess();
                return this.card;
            }
        }
    }
}
