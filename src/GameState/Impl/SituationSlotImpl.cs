namespace AutoccultistNS.GameState.Impl
{
    using System;
    using System.Linq;
    using SecretHistories.Spheres;

    internal class SituationSlotImpl : GameStateObject, ISituationSlot
    {
        private readonly string sphereId;
        private readonly ICardState card;
        private readonly Lazy<int> hashCode;

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

            this.hashCode = new Lazy<int>(() => HashUtils.Hash(this.sphereId, this.card));
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

        public override int GetHashCode()
        {
            return this.hashCode.Value;
        }
    }
}
