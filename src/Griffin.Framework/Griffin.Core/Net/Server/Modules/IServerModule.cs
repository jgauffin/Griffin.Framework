using System.Threading.Tasks;
using Griffin.Cqs.Net.Modules;

namespace Griffin.Net.Server.Modules
{
    /// <summary>
    ///     Used to process the inbound request to produce a response.
    /// </summary>
    /// <remarks>
    ///     If you would like to generate error pages or log errs then simply implement this interface and add it as a post
    /// </remarks>
    public interface IServerModule
    {
        /// <summary>
        ///     Begin request is always called for all modules.
        /// </summary>
        /// <param name="context">Context information</param>
        /// <returns>If message processing can continue</returns>
        Task BeginRequestAsync(IClientContext context);

        /// <summary>
        ///     Always called for all modules.
        /// </summary>
        /// <param name="context">Context information</param>
        /// <returns>If message processing can continue</returns>
        /// <remarks>
        ///     <para>
        ///         Check the <see cref="ModuleResult" /> property to see how the message processing have gone so
        ///         far.
        ///     </para>
        /// </remarks>
        Task EndRequest(IClientContext context);


        /// <summary>
        ///     ProcessAsync message
        /// </summary>
        /// <param name="context">Context information</param>
        /// <returns>If message processing can continue</returns>
        /// <remarks>
        ///     <para>
        ///         Check the <see cref="ModuleResult" /> property to see how the message processing have gone so
        ///         far.
        ///     </para>
        /// </remarks>
        Task<ModuleResult> ProcessAsync(IClientContext context);
    }
}