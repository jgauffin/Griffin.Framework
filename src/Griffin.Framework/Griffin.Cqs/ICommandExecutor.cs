using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs
{
    /// <summary>
    /// Allows us to wrap the generic interfaces with a non generic interface to simplify the different implementations.
    /// </summary>
    public interface ICommandExecutor
    {
        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="command">Command to execute</param>
        Task ExecuteAsync(Command command);
    }
}