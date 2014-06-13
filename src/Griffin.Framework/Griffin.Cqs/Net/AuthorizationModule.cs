using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Cqs.Net.Modules;
using Griffin.Net.Server.Modules;

namespace Griffin.Cqs.Net
{
    /// <summary>
    ///     Checks the <see cref="AuthorizeAttribute" /> on the current message to see if the logged in user have permission to
    ///     execute the message.
    /// </summary>
    public class AuthorizationModule : IServerModule
    {
        public async Task BeginRequestAsync(IClientContext context)
        {
        }

        public async Task EndRequest(IClientContext context)
        {
        }

        public async Task<ModuleResult> ProcessAsync(IClientContext context)
        {
            var attribute = context.RequestMessage.GetType().GetCustomAttribute<AuthorizeAttribute>();
            if (attribute == null)
                return ModuleResult.Continue;

            if (attribute.Roles.Any(role => Thread.CurrentPrincipal.IsInRole(role)))
            {
                return ModuleResult.Continue;
            }

            context.ResponseMessage =
                new AuthenticationException(string.Format("You are not allowed to invoke '{0}'.",
                    context.RequestMessage.GetType().Name));
            return ModuleResult.SendResponse;
        }
    }
}