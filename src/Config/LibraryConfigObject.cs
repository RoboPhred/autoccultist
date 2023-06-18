namespace AutoccultistNS.Config
{
    using System;
    using System.IO;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// A reference to a library-loaded config object.
    /// <para>
    /// Values should be a string file path, a config object id, or an !import directive.
    /// </summary>
    public class LibraryConfigObject<T> : IYamlConvertible where T : IConfigObject
    {
        public T Value { get; private set; }

        public LibraryConfigObject(T value)
        {
            this.Value = value;
        }

        public static implicit operator T(LibraryConfigObject<T> obj)
        {
            return obj.Value;
        }

        public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            string providedName;
            T value;

            // For backwards compatibility.
            if (ImportDeserializer.TryConsumeImport(parser, out var fileName))
            {
                providedName = FilesystemHelpers.GetRelativePath(fileName, Autoccultist.AssemblyDirectory);
                value = Library.GetByFilePath<T>(providedName);
            }
            else
            {
                providedName = parser.Consume<Scalar>().Value;

                if (providedName.StartsWith("./"))
                {
                    var resolved = Path.Combine(AutoccultistNS.Yaml.Deserializer.CurrentFilePath, providedName.Substring(2));
                    value = Library.GetByFilePath<T>(resolved);
                }
                else if (providedName.StartsWith("/"))
                {
                    var resolved = Path.Combine(Autoccultist.AssemblyDirectory, providedName.Substring(1));
                    value = Library.GetByFilePath<T>(resolved);
                }
                else
                {
                    value = Library.GetById<T>(providedName);
                }
            }

            if (value == null)
            {
                throw new YamlException($"Could not find {typeof(T).Name} with name '{providedName}' in the library.");
            }

            this.Value = value;
        }

        public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            throw new NotSupportedException();
        }
    }
}