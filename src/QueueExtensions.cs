namespace Autoccultist
{
    using System.Collections.Generic;

    /// <summary>
    /// Extensions for <see cref="Queue{T}"/>.
    /// </summary>
    internal static class QueueExtensions
    {
        /// <summary>
        /// Try to dequeue a value, or return default.
        /// </summary>
        /// <param name="queue">The queue to dequeue from.</param>
        /// <typeparam name="T">The item type in the queue.</typeparam>
        /// <returns>The dequeued item, or the default value if no item was queued.</returns>
        public static T DequeueOrDefault<T>(this Queue<T> queue)
        {
            if (queue.Count == 0)
            {
                return default;
            }

            return queue.Dequeue();
        }
    }
}
