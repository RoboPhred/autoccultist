namespace AutoccultistNS.Config
{
    using System;
    using AutoccultistNS.Yaml;

    /// <summary>
    /// A reference to a library-loaded config object.
    /// <para>
    /// Values should be a string file path, a config object id, or an !import directive.
    /// </summary>
    [CustomDeserializer(typeof(ObjectOrLibraryEntryDeserializer))]
    public class ObjectOrLibraryEntry<T> : IYamlValueWrapper
        where T : IConfigObject
    {
        public ObjectOrLibraryEntry()
        {
        }

        public ObjectOrLibraryEntry(T value)
        {
            this.Value = value;
        }

        public T Value { get; private set; }

        public static implicit operator ObjectOrLibraryEntry<T>(T value)
        {
            return new ObjectOrLibraryEntry<T>(value);
        }

        public static implicit operator T(ObjectOrLibraryEntry<T> obj)
        {
            if (obj.Value == null)
            {
                throw new NullReferenceException("ObjectOrLibraryEntry has a null value.");
            }

            return obj.Value;
        }

        object IYamlValueWrapper.Unwrap()
        {
            return this.Value;
        }

        public override string ToString()
        {
            return $"ObjectOrLibraryEntry<{typeof(T).Name}>({this.Value})";
        }
    }
}
