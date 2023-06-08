namespace AutoccultistNS.Yaml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Deserializes <see cref="OneOrMany"/> objects.
    /// </summary>
    public class OneOrManyDeserializer : INodeDeserializer
    {
        /// <inheritdoc/>
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (!expectedType.IsGenericType || expectedType.GetGenericTypeDefinition() != typeof(OneOrMany<>))
            {
                value = null;
                return false;
            }

            var subjectType = expectedType.GetGenericArguments()[0];


            // We need to handle this specially, as we need to tunnel our "OneOrMany" into the inner document.
            if (ImportDeserializer.TryConsumeImport(reader, out var filePath))
            {
                // We have a bit of a condundrum here.  This file might contain an array of items, in which case its meant for us,
                // or it might contain a single item, which could also be used elsewhere by things that are not OneOrMany.
                // This is an important distinction, since in the latter case, we must cache the individual item.
                // We make our parse attempt return a single item if we do not encounter a sequence, but still cache the OneOrMany
                // if what we got was a sequence.
                value = Deserializer.DeserializeFromParser(filePath, importParser =>
                {
                    // Expecting a basic, non fragment single document file.
                    importParser.Consume<StreamStart>();
                    importParser.Consume<DocumentStart>();
                    if (!importParser.Accept<SequenceStart>(out var _))
                    {
                        // This isnt a sequence, its a 'one'.
                        // it is critical that we do not wrap this in a OneOrMany in case others want to import this elsewhere
                        return nestedObjectDeserializer(importParser, subjectType);
                    }

                    // So this is a sequence, deserialize it into a OneOrMany.
                    // We can safely return this to have it cached, as what else is going to be parsing this?
                    var innerValue = this.DeserializeOneOrMany(importParser, expectedType, subjectType, nestedObjectDeserializer);
                    importParser.Consume<DocumentEnd>();
                    importParser.Consume<StreamEnd>();

                    return innerValue;
                });

                if (subjectType.IsAssignableFrom(value.GetType()))
                {
                    // We got a subject, which is a 'one'.  Wrap it in a OneOrMany
                    value = Activator.CreateInstance(expectedType, new[] { new[] { value } });
                }

                Autoccultist.Instance.LogWarn($"OneOrMany parse complete.  We got {value.GetType().FullName}");

                return true;
            }
            else
            {
                value = this.DeserializeOneOrMany(reader, expectedType, subjectType, nestedObjectDeserializer);
                return true;
            }
        }

        private object DeserializeOneOrMany(IParser reader, Type expectedType, Type subjectType, Func<IParser, Type, object> nestedObjectDeserializer)
        {
            Autoccultist.Instance.LogWarn($"Deserializing OneOrMany with current {reader.Current?.GetType().Name}");

            IEnumerable items;
            if (reader.TryConsume<SequenceStart>(out var test))
            {
                Autoccultist.Instance.LogWarn($"Deserializing OneOrMany in sequence.  Start was {test.Start} with tag {test.Tag}");
                var sequenceItems = new List<object>();
                while (!reader.TryConsume<SequenceEnd>(out var _))
                {
                    Autoccultist.Instance.LogWarn($"Deserializing OneOrMany in sequence with item {reader.Current?.GetType().Name} Start was {reader.Current?.Start}");
                    sequenceItems.Add(nestedObjectDeserializer(reader, subjectType));
                }

                items = sequenceItems;
            }
            else
            {
                Autoccultist.Instance.LogWarn($"Deserializing OneOrMany in single with item {reader.Current?.GetType().Name}");
                var item = nestedObjectDeserializer(reader, subjectType);
                var array = Array.CreateInstance(subjectType, 1);
                array.SetValue(item, 0);
                items = array;
            }

            var result = Activator.CreateInstance(expectedType, new[] { items });
            Autoccultist.Instance.LogWarn($"OneOrMany DeserializeOneOrMany complete with {result.GetType().FullName}");

            return result;
        }
    }
}
