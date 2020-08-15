namespace Autoccultist.Yaml
{
    using System;
    using System.Collections.Generic;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;

    /// <summary>
    /// A parser meant to replay previously parsed events.
    /// </summary>
    internal class ReplayParser : IParser
    {
        private readonly List<ParsingEvent> replayEvents = new List<ParsingEvent>();

        private IEnumerator<ParsingEvent> replayEnumerator = null;

        /// <inheritdoc/>
        public ParsingEvent Current => this.replayEnumerator != null ? this.replayEnumerator.Current : throw new InvalidOperationException("ReplayParser has not been activated.");

        /// <summary>
        /// Enqueue a parsing event to emit when the parser starts.
        /// </summary>
        /// <param name="parsingEvent">The event to enqueue.</param>
        public void Enqueue(ParsingEvent parsingEvent)
        {
            if (this.replayEnumerator != null)
            {
                throw new InvalidOperationException("Cannot enqueue to a ReplayParser that has already been started.");
            }

            this.replayEvents.Add(parsingEvent);
        }

        /// <summary>
        /// Start the parser for its parse operations.
        /// </summary>
        public void Start()
        {
            if (this.replayEnumerator != null)
            {
                throw new InvalidOperationException("This ReplayParser has already been started.");
            }

            this.replayEnumerator = this.replayEvents.GetEnumerator();
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            return this.replayEnumerator.MoveNext();
        }
    }
}
