using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs
{
    /// <summary>
    /// Allows us to wrap the generic interfaces with a non generic interface to simplify the different implementations.
    /// </summary>
    public interface IEventExecutor
    {
        /// <summary>
        /// Execute query and get response.
        /// </summary>
        /// <param name="appEvent">Event to execute</param>
        Task ExecuteAsync(ApplicationEvent appEvent);
    }
}