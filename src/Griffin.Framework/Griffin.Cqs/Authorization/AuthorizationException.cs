using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Cqs.Authorization
{
    /// <summary>
    /// Failed to authorize user
    /// </summary>
    public class AuthorizationException : Exception
    {
        /// <summary>
        /// Create a new instance of <see cref="AuthorizationException"/>.
        /// </summary>
        /// <param name="cqsObjectType">The command/query/event/request object</param>
        /// <param name="requiredRoles">Roles that the user must have.</param>
        public AuthorizationException(Type cqsObjectType, params string[] requiredRoles)
            : base("'" + cqsObjectType.Name + "' requires the following role(s): " + string.Join(", ", requiredRoles))
        {
            
        }
    }
}
