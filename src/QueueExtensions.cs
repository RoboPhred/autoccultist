
using System.Collections.Generic;

namespace Autoccultist
{
    static class QueueExtensions
    {
        public static T TryDequeue<T>(this Queue<T> queue)
        {
            if (queue.Count == 0)
            {
                return default(T);
            }
            return queue.Dequeue();
        }
    }
}