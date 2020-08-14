namespace Autoccultist.Brain.Config
{
    public class TimeComparison
    {
        public float? GreaterThan { get; set; }
        public float? LessThan { get; set; }

        public void Validate()
        {
            if (this.GreaterThan.HasValue == this.LessThan.HasValue)
            {
                throw new InvalidConfigException("Time comparison must either have a greaterThan or lessThan, but not both.");
            }
        }

        public bool IsMatch(float value)
        {
            if (this.GreaterThan.HasValue)
            {
                return value > this.GreaterThan.Value;
            }
            if (this.LessThan.HasValue)
            {
                return value < this.LessThan.Value;
            }
            return false;
        }
    }
}