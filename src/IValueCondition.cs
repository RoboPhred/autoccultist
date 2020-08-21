namespace Autoccultist
{
    /// <summary>
    /// An interface describing an object that compares a value to a condition.
    /// </summary>
    public interface IValueCondition
    {
        /// <summary>
        /// Tests to see if the condition is met by the given value.
        /// </summary>
        /// <param name="value">The value to compare.</param>
        /// <returns>True if the condition is met, otherwise False.</returns>
        bool IsConditionMet(float value);
    }
}
