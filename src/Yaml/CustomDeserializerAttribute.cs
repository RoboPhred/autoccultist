namespace AutoccultistNS.Yaml
{
    using System;
    using System.Reflection;
    using YamlDotNet.Serialization;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CustomDeserializerAttribute : Attribute
    {
        private readonly Type deserializer;

        public CustomDeserializerAttribute(Type deserializer)
        {
            this.deserializer = deserializer;
        }

        public static INodeDeserializer GetDeserializer(Type type)
        {
            var attr = type.GetCustomAttribute<CustomDeserializerAttribute>();
            if (attr == null)
            {
                return null;
            }

            if (!typeof(INodeDeserializer).IsAssignableFrom(attr.deserializer))
            {
                throw new InvalidOperationException($"Custom deserializer {attr.deserializer.Name} does not implement {nameof(INodeDeserializer)}.");
            }

            return Activator.CreateInstance(attr.deserializer) as INodeDeserializer;
        }
    }
}
