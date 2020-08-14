using System;
using System.Collections.Generic;
using System.Linq;

namespace Autoccultist.Yaml
{
    [AttributeUsage(AttributeTargets.Class)]
    class DuckTypeKeysAttribute : Attribute
    {
        public IReadOnlyList<string> Keys { get; private set; }

        public DuckTypeKeysAttribute(string[] keys)
        {
            this.Keys = keys.ToList();
        }
    }
}