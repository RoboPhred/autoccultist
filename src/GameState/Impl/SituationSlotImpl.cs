namespace AutoccultistNS.GameState.Impl
{
    using System.Linq;
    using SecretHistories.Spheres;

    internal class SituationSlotImpl : GameStateObject, ISituationSlot
    {
        private readonly string sphereId;
        private readonly ICardState card;

        /// <summary>
        /// Initializes a new instance of the <see cref="SituationSlotImpl"/> class.
        /// </summary>
        /// <param name="sphere">The sphere to represent.</param>
        /// <param name="situation">The situation the sphere is in.</param>
        public SituationSlotImpl(Sphere sphere, string situation)
        {
            this.sphereId = sphere.GoverningSphereSpec.Id;

            var elementStack = sphere.GetElementStacks().FirstOrDefault();
            if (elementStack != null)
            {
                this.card = CardStateImpl.CardStatesFromStack(elementStack, CardLocation.Slotted, situation).First();
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
                return this.sphereId;
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

        protected override int ComputeContentHash()
        {
            return HashUtils.Hash(this.sphereId, this.card);
        }
    }
}
