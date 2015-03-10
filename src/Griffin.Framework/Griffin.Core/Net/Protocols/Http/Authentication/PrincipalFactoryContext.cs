using System;

namespace Griffin.Net.Protocols.Http.Authentication
{
    /// <summary>
    /// Context for <see cref="IPrincipalFactory"/>.
    /// </summary>
    public class PrincipalFactoryContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrincipalFactoryContext" /> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="userName">user that have authenticated successfully.</param>
        public PrincipalFactoryContext(IHttpRequest request, IAuthenticationUser userName)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (userName == null) throw new ArgumentNullException("userName");

            UserName = userName;
            Request = request;
        }

        /// <summary>
        /// Gets the name of the user which was provided by the <see cref="IAccountService"/>.
        /// </summary>
        public IAuthenticationUser UserName { get; private set; }

        /// <summary>
        /// Gets the HTTP request.
        /// </summary>
        public IHttpRequest Request { get; private set; }
    }
}