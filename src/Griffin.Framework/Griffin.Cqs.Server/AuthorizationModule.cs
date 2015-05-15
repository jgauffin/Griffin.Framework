using System.Reflection;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Cqs.Authorization;
using Griffin.Net.LiteServer.Modules;

namespace Griffin.Cqs.Server
{
    /// <summary>
    ///     Checks the <see cref="AuthorizeAttribute" /> on the current message to see if the logged in user have permission to
    ///     execute the message.
    /// </summary>
    public class AuthorizationModule : IServerModule
    {
#pragma warning disable 1998
        public async Task BeginRequestAsync(IClientContext context)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        public async Task EndRequest(IClientContext context)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        public async Task<ModuleResult> ProcessAsync(IClientContext context)
#pragma warning restore 1998
        {
            var attributes = context.RequestMessage.GetType().GetCustomAttributes<AuthorizeAttribute>();
            foreach (var attribute in attributes)
            {
                var entityWithRoles = context.RequestMessage as IEntityWithRoles;
                if (entityWithRoles != null)
                {
                    foreach (var role in entityWithRoles.Roles)
                    {
                        if (Thread.CurrentPrincipal.IsInRole(role))
                            continue;

                        context.ResponseMessage =
                            new AuthenticationException(
                                string.Format(
                                    "You are not allowed to invoke '{0}', as you are not part of role '{1}'.",
                                    context.RequestMessage.GetType().Name, role));
                        return ModuleResult.SendResponse;
                    }
                }

                foreach (var role in attribute.Roles)
                {
                    if (Thread.CurrentPrincipal.IsInRole(role))
                        continue;

                    context.ResponseMessage =
                                new AuthenticationException(
                                    string.Format(
                                        "You are not allowed to invoke '{0}', as you are not part of role '{1}'.",
                                        context.RequestMessage.GetType().Name, role));
                    return ModuleResult.SendResponse;
                }
            }
            return ModuleResult.Continue;
        }
    }
}