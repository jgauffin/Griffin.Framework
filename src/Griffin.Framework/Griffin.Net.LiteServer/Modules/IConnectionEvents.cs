using System.Threading.Tasks;

namespace Griffin.Net.LiteServer.Modules
{
    /// <summary>
    ///     You can implement this interface for your modules to be able to detect disconnects and connects.
    /// </summary>
    public interface IConnectionEvents
    {
        /// <summary>
        ///     Client have just connected
        /// </summary>
        /// <param name="context">No request message have been assigned as this is just the connect event</param>
        /// <returns>Task to wait upon for completion</returns>
        /// <remarks>
        ///     You can use this event to send a message to the client
        /// </remarks>
        Task<ModuleResult> OnClientConnected(IClientContext context);

        /// <summary>
        ///     client have been disconnected
        /// </summary>
        /// <param name="context">No request nor response may be used.</param>
        /// <returns>Task to wait upon for completion</returns>
        Task OnClientDisconnect(IClientContext context);
    }
}