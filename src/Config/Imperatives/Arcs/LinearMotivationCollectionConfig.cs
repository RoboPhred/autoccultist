namespace AutoccultistNS.Config
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.GameState;

    public class LinearMotivationCollectionConfig : MotivationCollectionConfig, IList<MotivationConfig>
    {
        private readonly List<MotivationConfig> motivations = new();

        public bool IsReadOnly => false;

        public override int Count => this.motivations.Count;

        public MotivationConfig this[int index]
        {
            get
            {
                return this.motivations[index];
            }

            set
            {
                this.motivations[index] = value;
            }
        }

        public override ConditionResult CanActivate(IGameState state)
        {
            return ConditionResult.Success;
        }

        public void Add(MotivationConfig item)
        {
            this.motivations.Add(item);
        }

        public void Clear()
        {
            this.motivations.Clear();
        }

        public bool Contains(MotivationConfig item)
        {
            return this.motivations.Contains(item);
        }

        public void CopyTo(MotivationConfig[] array, int arrayIndex)
        {
            this.motivations.CopyTo(array, arrayIndex);
        }

        public IEnumerator<MotivationConfig> GetEnumerator()
        {
            return this.motivations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.motivations.GetEnumerator();
        }

        public int IndexOf(MotivationConfig item)
        {
            return this.motivations.IndexOf(item);
        }

        public void Insert(int index, MotivationConfig item)
        {
            this.motivations.Insert(index, item);
        }

        public bool Remove(MotivationConfig item)
        {
            return this.motivations.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this.motivations.RemoveAt(index);
        }

        public override IEnumerable<IImperative> Flatten()
        {
            return this.motivations.SelectMany(x => x.Flatten());
        }

        protected override IEnumerable<IMotivationConfig> GetCurrentMotivations(IGameState state)
        {
            var activeMotivation = this.motivations.FirstOrDefault(x => !x.IsSatisfied(state));
            if (activeMotivation != null)
            {
                yield return activeMotivation;
            }
        }
    }
}
