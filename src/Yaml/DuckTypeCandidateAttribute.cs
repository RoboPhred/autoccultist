using System;
using System.Collections.Generic;
using System.Linq;

namespace Autoccultist.Yaml
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
    class DuckTypeCandidateAttribute : Attribute
    {
        public Type CandidateType { get; private set; }

        public DuckTypeCandidateAttribute(Type candidateType)
        {
            this.CandidateType = candidateType;
        }

        public static IList<Type> GetDuckCandidates(Type type)
        {
            var candidateAttributes = (DuckTypeCandidateAttribute[])type.GetCustomAttributes(typeof(DuckTypeCandidateAttribute), false);
            return candidateAttributes.Select(attr => attr.CandidateType).ToList();
        }
    }
}