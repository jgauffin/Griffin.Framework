using System.Linq;
using System.Reflection;
using System.Threading;

namespace Griffin.Cqs.Authorization
{
    /// <summary>
    ///     Uses the <see cref="AuthorizeAttribute" /> to authorize users
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Either the CQS object (like a command) or the handler can have been tagged with the attribute.
    ///     </para>
    ///     <para>
    ///         Assign it to the <see cref="GlobalConfiguration.AuthorizationFilter" /> property to activate.
    ///     </para>
    /// </remarks>
    public class RoleAuthorizer : IAuthorizationFilter
    {
        /// <summary>
        ///     Authorize context.
        /// </summary>
        /// <param name="context">Contains information about the object being executed.</param>
        public void Authorize(AuthorizationFilterContext context)
        {
            var principal = Thread.CurrentPrincipal;

            var attr = context.CqsObject.GetType().GetCustomAttribute<AuthorizeAttribute>();
            if (attr != null && !attr.Roles.All(principal.IsInRole))
                throw new AuthorizationException(context.CqsObject.GetType(), attr.Roles);

            foreach (var handler in context.Handlers)
            {
                attr = handler.GetType().GetCustomAttribute<AuthorizeAttribute>();
                if (attr != null && !attr.Roles.All(principal.IsInRole))
                    throw new AuthorizationException(context.CqsObject.GetType(), attr.Roles);
            }
        }
    }
}