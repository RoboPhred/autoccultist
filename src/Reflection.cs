namespace AutoccultistNS
{
    /// <summary>
    /// Reflection utilities.
    /// </summary>
    public static class Reflection
    {
        /// <summary>
        /// Gets the value of a private field.
        /// </summary>
        /// <param name="instance">The instance to read the field from.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <returns>The value of the private field.</returns>
        public static T GetPrivateField<T>(object instance, string fieldName)
        {
            var prop = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)prop.GetValue(instance);
        }

        /// <summary>
        /// Gets the value of a private field.
        /// </summary>
        /// <param name="instance">The instance to read the field from.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to set the field to.</param>
        /// <typeparam name="T">The type of the field.</typeparam>
        public static void SetPrivateField<T>(object instance, string fieldName, T value)
        {
            var prop = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prop.SetValue(instance, value);
        }
    }
}
