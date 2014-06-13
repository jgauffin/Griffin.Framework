using System.Threading.Tasks;
using Griffin.Cqs.Net.Modules;
using Griffin.Net.Server.Modules;

namespace Griffin.Cqs.Demo
{
    public class AuthenticationModule : IServerModule
    {
        /// <summary>
        ///     Begin request is always called for all modules.
        /// </summary>
        /// <param name="context">Context information</param>
        /// <returns>If message processing can continue</returns>
        public Task BeginRequestAsync(IClientContext context)
        {
            
        }

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
        public Task EndRequest(IClientContext context)
        {
            
        }

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
        public Task<ModuleResult> ProcessAsync(IClientContext context)
        {
            if (context.ResponseMessage is Authus)
        }
    }
}