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

            NoonUtility.LogWarning($"Deserializing type {expectedType} at {AutoccultistNS.Yaml.Deserializer.CurrentFilePath}:{reader.Current?.Start}");


            // FIXME: We return values wrapped in a ObjectOrLibraryEntry, as we are forced to.
            // However, this means values cache as that.  This is bullshit.
            // We introduced IYamlValueUnwrapper as a nasty hack to work around it.

            var subjectType = expectedType.GetGenericArguments()[0];
            var start = reader.Current?.Start;

            if (reader.TryConsume<Scalar>(out var scalar))
            {
                NoonUtility.LogWarning($"Deserializing scalar {scalar.Value} at {AutoccultistNS.Yaml.Deserializer.CurrentFilePath}:{reader.Current?.Start}");
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
                    throw new YamlFileException(AutoccultistNS.Yaml.Deserializer.CurrentFilePath, start, reader.Current?.End, $"Could not find {subjectType.Name} with name '{providedName}' in the library.");
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
