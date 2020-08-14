using System;
using System.Collections.Generic;
using System.Linq;
using Autoccultist.src.Brain.Util;
using Autoccultist.Yaml;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Autoccultist.Brain.Config
{
    [DuckTypeKeys(new[] { "allOf", "anyOf", "oneOf" })]
    public class CompoundCondition : IGameStateConditionConfig, IYamlConvertible
    {
        public ConditionMode Mode { get; set; }
        public List<IGameStateConditionConfig> Requirements { get; set; } = new List<IGameStateConditionConfig>();

        public void Validate()
        {
            if (this.Requirements == null || this.Requirements.Count == 0)
            {
                throw new InvalidConfigException("CompoundCondition must have requirements.");
            }

            foreach (var requirement in this.Requirements)
            {
                requirement.Validate();
            }
        }

        public bool IsConditionMet(IGameState state)
        {
            switch(this.Mode)
            {
            case ConditionMode.AllOf:
                try
                {
                    // Expand all compound conditions into base conditions
                    List<IBaseCondition> requirements = new List<IBaseCondition>();
                    foreach(IGameStateConditionConfig condition in this.Requirements)
                    {
                        if (condition is IBaseCondition bc)
                        {
                            requirements.Add(bc);
                        }
                        else if (condition is CompoundCondition cg)
                        {
                            requirements.AddRange(cg.GetAllBaseConditions(state));
                        }
                    }

                    // Parse the base requirements and verify they can all be satisfied.
                    CardSetCondition cardsRequired = new CardSetCondition();
                    HashSet<String> situationSet = new HashSet<String>();
                    foreach (IBaseCondition condition in requirements)
                    {
                        switch (condition)
                        {
                        case SituationCondition sc:
                            if (sc.State == SituationStateConfig.Unstarted)
                            {
                                if (situationSet.Contains(sc.SituationId))
                                {
                                    return false;
                                }
                                situationSet.Add(sc.SituationId);
                            }
                            else if(!sc.IsConditionMet(state))
                            {
                                return false;
                            }
                            break;
                        case CardChoice cc:
                            cardsRequired.Add(cc);
                            break;
                        case CardSetCondition csc:
                            cardsRequired.AddRange(csc);
                            break;
                        default:
                            throw new NotImplementedException($"Condition type {condition.GetType()} is not implemented.");
                        }
                    }

                    return cardsRequired.IsConditionMet(state);
                }
                catch(Exception e)
                {
                    AutoccultistPlugin.Instance.LogWarn(e.Message);
                    return false;
                }

            case ConditionMode.AnyOf:
                return this.Requirements.Any(condition => condition.IsConditionMet(state));
            case ConditionMode.NoneOf:
                return !this.Requirements.Any(condition => condition.IsConditionMet(state));
            default:
                throw new NotImplementedException($"Condition mode {this.Mode} is not implemented.");
            }
        }

        private IEnumerable<IBaseCondition> GetAllBaseConditions(IGameState state)
        {
            switch(this.Mode)
            {
            case ConditionMode.AllOf:
                List<IBaseCondition> list = new List<IBaseCondition>();
                foreach(IGameStateConditionConfig condition in Requirements)
                {
                    switch(condition)
                    {
                    case CompoundCondition gsc:
                        list.AddRange(gsc.GetAllBaseConditions(state));
                        break;
                    case IBaseCondition bc:
                        list.Add(bc);
                        break;
                    }
                }
                return list;

            case ConditionMode.NoneOf:
                if(this.IsConditionMet(state))
                {
                    return new IBaseCondition[0];
                }

                throw new ConditionNotMetException("A 'NoneOf' requirement was not met: " + this.ToString());

            case ConditionMode.AnyOf:
                throw new NotImplementedException("'AnyOf' CompoundConditions are not (currently) allowed to be children of 'AllOf' CompoundConditions.");
                
            default:
                throw new NotImplementedException($"Condition mode {this.Mode} is not implemented.");
            }
        }

        void IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            parser.Consume<MappingStart>();

            var key = parser.Consume<Scalar>();
            switch(key.Value)
            {
            case "allOf":
                this.Mode = ConditionMode.AllOf;
                break;
            case "anyOf":
                this.Mode = ConditionMode.AnyOf;
                break;
            case "noneOf":
                this.Mode = ConditionMode.NoneOf;
                break;
            default:
                throw new YamlException(key.Start, key.End, "GameStateCondition must have one of the following keys: \"allOf\", \"anyOf\", \"oneOf\".");
            }

            this.Requirements = (List<IGameStateConditionConfig>)nestedObjectDeserializer(typeof(List<IGameStateConditionConfig>));

            if(parser.Accept<Scalar>(out var _))
            {
                throw new YamlException(key.Start, key.End, "GameStateCondition must only have one property.");
            }

            parser.Consume<MappingEnd>();
        }

        void IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            throw new NotSupportedException();
        }
    }

    public enum ConditionMode
    {
        AnyOf,
        AllOf,
        NoneOf
    }
}