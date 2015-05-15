using System;
using System.Collections.Generic;

namespace Griffin.Cqs.Authorization
{
    /// <summary>
    ///     Context for <see cref="IAuthorizationFilter" />
    /// </summary>
    public class AuthorizationFilterContext
    {
        /// <summary>
        ///     Create a new instance if <see cref="AuthorizationFilterContext" />.
        /// </summary>
        /// <param name="cqsObject">Command/query/event/request to be executed</param>
        /// <param name="handlers">List of identified handlers</param>
        public AuthorizationFilterContext(object cqsObject, IReadOnlyList<object> handlers)
        {
            if (cqsObject == null) throw new ArgumentNullException("cqsObject");
            if (handlers == null) throw new ArgumentNullException("handlers");
            Handlers = handlers;
            CqsObject = cqsObject;
        }

        /// <summary>
        ///     Command/query/event/request to be executed
        /// </summary>
        public object CqsObject { get; private set; }

        /// <summary>
        ///     List of identified handlers.
        /// </summary>
        public IReadOnlyList<object> Handlers { get; private set; }
    }
}