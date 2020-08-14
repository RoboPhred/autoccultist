using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Autoccultist.Yaml
{
    class DuckTypeDeserializer : INodeDeserializer
    {
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            var candidates = DuckTypeCandidateAttribute.GetDuckCandidates(expectedType);
            if (candidates.Count == 0)
            {
                // Not duck typable.
                value = null;
                return false;
            }

            // We need to parse this object in its entirity to discover its keys.
            //  We also need to remember what we have parsed so that nestedObjectDeserializer can do its thing.
            // This replay parser will remember everything we parsed, and re-parse it for nestedObjectDeserializer.
            var replayParser = new ReplayParser();

            var mappingStart = reader.Consume<MappingStart>();
            replayParser.Enqueue(mappingStart);

            var keys = new List<string>();
            MappingEnd mappingEnd;
            while (!reader.TryConsume<MappingEnd>(out mappingEnd))
            {
                var key = reader.Consume<Scalar>();
                keys.Add(key.Value);
                replayParser.Enqueue(key);

                // The value might be more complex than just a scaler
                //  This code is cribbed from the SkipThisAndNestedEvents IParser extension method
                var depth = 0;
                do
                {
                    var next = reader.Consume<ParsingEvent>();
                    depth += next.NestingIncrease;

                    // Make sure to save this node for the nestedObjectDeserializer pass
                    replayParser.Enqueue(next);
                }
                while (depth > 0);
            }

            // reader.Accept will have obtained a mapping end, queue it up.
            replayParser.Enqueue(mappingEnd);

            // Pick the type based on the keys we have found.
            var chosenType = ChooseType(candidates, keys);
            if (chosenType == null)
            {
                // We have candidate types, but we were unable to find a match.
                throw new YamlException(
                    mappingStart.Start,
                    mappingEnd.End,
                    $"Cannot identify instance type for {expectedType.Name} based on its properties {string.Join(", ", keys)}.  Must be one of: {string.Join(", ", candidates.Select(x => x.Name))}"
                );
            }

            replayParser.Start();
            value = nestedObjectDeserializer(replayParser, chosenType);
            return true;
        }

        // Choose a candidate duck type based on the keys we posess.
        //  The best match is the one that has the most keys in common.
        private static Type ChooseType(IList<Type> candidates, IList<string> keys)
        {
            var matches =
                from candidate in candidates
                let yamlProperties = GetTypeYamlProperties(candidate)
                // Must not have any non-matching keys.
                where !keys.Except(yamlProperties).Any()
                // How many candidate keys match the keys we have?
                let matchCount = yamlProperties.Intersect(keys).Count()
                // We are only interested if we have at least one match.
                where matchCount > 0
                // Find the candidate with the most matches
                orderby matchCount descending
                select candidate;

            return matches.FirstOrDefault();
        }

        private static IReadOnlyList<string> GetTypeYamlProperties(Type type)
        {
            var keysAttribute = (DuckTypeKeysAttribute)type.GetCustomAttribute(typeof(DuckTypeKeysAttribute));
            if (keysAttribute != null)
            {
                // Attribute overrides key discovery
                return keysAttribute.Keys;
            }

            // YamlDotNet handles any public instance properties or fields.

            var properties =
                from property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                let yamlAttribute = property.GetCustomAttribute<YamlMemberAttribute>()
                let name = yamlAttribute?.Alias ?? Deserializer.NamingConvention.Apply(property.Name)
                select name;

            var fields =
                from field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                let yamlAttribute = field.GetCustomAttribute<YamlMemberAttribute>()
                let name = yamlAttribute?.Alias ?? Deserializer.NamingConvention.Apply(field.Name)
                select name;

            var keys = properties.Concat(fields).ToList();
            return keys;
        }
    }
}