using System.Threading.Tasks;

namespace Griffin
{
    /// <summary>
    /// A queue definition
    /// </summary>
    /// <typeparam name="T">Type of entity to store in the queue</typeparam>
    public interface IQueue<T>
    {
        /// <summary>
        /// Dequeue an item from our queue.
        /// </summary>
        /// <returns>Dequeued item; <c>default(T)</c> if there are no more items in the queue.</returns>
        Task<T> DequeueAsync();

        /// <summary>
        /// Enqueue item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task EnqueueAsync(T item);
    }
}