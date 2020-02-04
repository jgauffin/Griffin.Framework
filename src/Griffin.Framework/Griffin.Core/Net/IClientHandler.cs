using System.Threading.Tasks;
using Griffin.Net.Authentication.Messages;

namespace Griffin.Net
{
    /// <summary>
    /// Handler for a specific endpoint.
    /// </summary>
    /// <typeparam name="TContext">Type of context used to manage endpoint specific information</typeparam>
    public interface IClientHandler<TContext> where TContext : IMiddlewareContext
    {

        /// <summary>
        /// Used to process all communication. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <remarks>
        ///<para>
        ///Should not exit until all messages have been processed (i.e. not before the client disconnects or something goes wrong).
        /// </para>
        /// </remarks>
        Task ProcessAsync(MessagingServerPipeline<TContext> context);

        /// <summary>
        /// Abort processing and close connection.
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();

        void Reset();
    }
}