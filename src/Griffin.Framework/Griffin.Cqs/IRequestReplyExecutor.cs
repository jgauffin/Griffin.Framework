using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs
{
    /// <summary>
    /// Allows us to wrap the generic interfaces with a non generic interface to simplify the different implementations.
    /// </summary>
    public interface IRequestReplyExecutor
    {
        /// <summary>
        /// Execute request and return reply.
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Reply</returns>
        Task<object> ExecuteAsync(IRequest request);
    }
}