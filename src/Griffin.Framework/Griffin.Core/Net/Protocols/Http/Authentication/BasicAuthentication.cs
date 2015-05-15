using System;
using System.Net;
using System.Text;
using Griffin.Net.Protocols.Http.Authentication.Digest;

namespace Griffin.Net.Protocols.Http.Authentication
{
    /// <summary>
    /// Basic aithentication
    /// </summary>
    public class BasicAuthentication : IAuthenticator
    {
        private readonly string _realm;
        private readonly IAccountService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicAuthentication" /> class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <param name="realm">The realm.</param>
        /// <exception cref="System.ArgumentNullException">
        /// userService
        /// or
        /// realm
        /// </exception>
        public BasicAuthentication(IAccountService userService, string realm)
        {
            if (userService == null) throw new ArgumentNullException("userService");
            if (realm == null) throw new ArgumentNullException("realm");
            _userService = userService;
            _realm = realm;
        }

        /// <summary>
        /// Gets authenticator scheme
        /// </summary>
        /// <value></value>
        /// <example>
        /// digest
        /// </example>
        public string Scheme
        {
            get { return "basic"; }
        }

        #region IAuthenticator Members

        /// <summary>
        /// Create a WWW-Authenticate header
        /// </summary>
        public void CreateChallenge(IHttpRequest httpRequest, IHttpResponse response)
        {
            response.AddHeader("WWW-Authenticate", "Basic realm=\"" + _realm + "\"");
            response.StatusCode = 401;
        }

        /// <summary>
        /// Gets name of the authentication scheme
        /// </summary>
        /// <remarks>"BASIC", "DIGEST" etc.</remarks>
        public string AuthenticationScheme
        {
            get { return "basic"; }
        }

        /// <summary>
        /// Authorize a request.
        /// </summary>
        /// <param name="request">Request being authenticated</param>
        /// <returns>Authenticated user if successful; otherwise null.</returns>
        public IAuthenticationUser Authenticate(IHttpRequest request)
        {
            var authHeader = request.Headers["Authorization"];
            if (authHeader == null)
                return null;

            authHeader = authHeader.Remove(0, "Basic ".Length);

            /*
             * To receive authorization, the client sends the userid and password,
                separated by a single colon (":") character, within a base64 [7]
                encoded string in the credentials.*/
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader));
            var pos = decoded.IndexOf(':');
            if (pos == -1)
                throw new BadRequestException("Invalid basic authentication header, failed to find colon. Got: " +
                                              authHeader);

            var password = decoded.Substring(pos + 1, decoded.Length - pos - 1);
            var userName = decoded.Substring(0, pos);

            var user = _userService.Lookup(userName, request.Uri);
            if (user == null)
                return null;

            if (user.Password == null && user.HA1 == null)
            {
                if (!_userService.ComparePassword(user, password))
                    throw new HttpException(HttpStatusCode.Unauthorized, "Incorrect username or password");
            }
            else if (user.Password == null)
            {
                var ha1 = DigestAuthenticator.GetHa1(request.Uri.Host, userName, password);
                if (ha1 != user.HA1)
                    throw new HttpException(HttpStatusCode.Unauthorized, "Incorrect username or password");
            }
            else
            {
                if (password != user.Password)
                    throw new HttpException(HttpStatusCode.Unauthorized, "Incorrect username or password");
            }

            return user;
        }

        #endregion
    }
}