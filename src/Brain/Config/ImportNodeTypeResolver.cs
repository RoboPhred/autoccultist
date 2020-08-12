using System;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Autoccultist.Brain.Config
{
    class ImportNodeTypeResolver : INodeTypeResolver
    {
        public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
        {
            if (nodeEvent.Tag == "!import")
            {
                // Leave curent type alone.
                return true;
            }
            return false;
        }
    }
}