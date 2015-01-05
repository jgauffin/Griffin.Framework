using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Griffin
{
    /// <summary>
    ///     Wraps <c><![CDATA[ConcurrentQueue<T>]]></c>
    /// </summary>
    /// <typeparam name="T">Type of item to store.</typeparam>
    public class MemoryQueue<T> : IQueue<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();


        /// <summary>
        ///     Dequeue an item from our queue.
        /// </summary>
        /// <returns>Dequeued item; <c>default(T)</c> if there are no more items in the queue.</returns>
        public Task<T> DequeueAsync()
        {
            T item;
            return _queue.TryDequeue(out item)
                ? Task.FromResult(item)
                : Task.FromResult(default(T));
        }

        /// <summary>
        ///     Enqueue item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
#pragma warning disable 1998
        public async Task EnqueueAsync(T item)
#pragma warning restore 1998
        {
            _queue.Enqueue(item);
        }
    }
}
