using System.Threading.Tasks;

namespace Griffin.IO
{
    public interface IQueue<T>
    {
        Task<T> DequeueAsync();
        Task EnqueueAsync<T>(T item);
    }
}