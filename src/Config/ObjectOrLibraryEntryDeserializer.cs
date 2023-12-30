namespace AutoccultistNS.Config
{
    using System;
    using System.IO;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    public class ObjectOrLibraryEntryDeserializer : INodeDeserializer
    {
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (!expectedType.IsGenericType || expectedType.GetGenericTypeDefinition() != typeof(ObjectOrLibraryEntry<>))
            {
                value = null;
                return false;
            }

            var subjectType = expectedType.GetGenericArguments()[0];
            var start = reader.Current?.Start;

            if (reader.TryConsume<Scalar>(out var scalar))
            {
                var providedName = scalar.Value;
                object data;

                if (providedName.StartsWith("./"))
                {
                    var resolved = Path.Combine(AutoccultistNS.Yaml.Deserializer.CurrentFilePath, providedName.Substring(2));
                    data = Library.GetByFilePath(subjectType, resolved);
                }
                else if (providedName.StartsWith("/"))
                {
                    var resolved = Path.Combine(Autoccultist.AssemblyDirectory, providedName.Substring(1));
                    data = Library.GetByFilePath(subjectType, resolved);
                }
                else
                {
                    data = Library.GetById(subjectType, providedName);
                }

                if (data == null)
                {
                    throw new SemanticErrorException(start ?? Mark.Empty, reader.Current?.End ?? Mark.Empty, $"Could not find {subjectType.Name} with name '{providedName}' in the library.");
                }

                value = Activator.CreateInstance(expectedType, new[] { data });
                return true;
            }
            else
            {
                var data = nestedObjectDeserializer(reader, subjectType);
                value = Activator.CreateInstance(expectedType, new[] { data });
                return true;
            }
        }
    }
}
