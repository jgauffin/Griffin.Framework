using System.Reflection;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Cqs.Authorization;
using Griffin.Net.LiteServer.Modules;
#pragma warning disable 1998

namespace Griffin.Cqs.Server
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
                                new AuthorizationException(context.RequestMessage.GetType(), role);
                    return ModuleResult.SendResponse;
                }
            }
            return ModuleResult.Continue;
        }
    }
}

#pragma warning restore 1998