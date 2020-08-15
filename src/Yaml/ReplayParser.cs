using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Autoccultist.Yaml
{
    class ReplayParser : IParser
    {
        private List<ParsingEvent> replayEvents = new List<ParsingEvent>();
        private IEnumerator<ParsingEvent> replayEnumerator = null;

        public ParsingEvent Current => this.replayEnumerator != null ? this.replayEnumerator.Current : throw new InvalidOperationException("ReplayParser has not been activated.");

        public void Enqueue(ParsingEvent parsingEvent)
        {
            if (this.replayEnumerator != null)
            {
                throw new InvalidOperationException("Cannot enqueue to a ReplayParser that has already been started.");
            }

            this.replayEvents.Add(parsingEvent);
        }

        public void Start()
        {
            if (this.replayEnumerator != null)
            {
                throw new InvalidOperationException("This ReplayParser has already been started.");
            }

            this.replayEnumerator = this.replayEvents.GetEnumerator();
        }

        public bool MoveNext()
        {
            return this.replayEnumerator.MoveNext();
        }
    }
}
