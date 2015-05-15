using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Griffin.Net.Protocols.Http.Authentication
{
    /// <summary>
    ///     Authenticates using a HTTP cookie
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Do note that this implementation hashes the user IP address. That means that the cookie is not valid if the
    ///         user gets a new IP. This is done
    ///         to prevent cookie hijacking.
    ///     </para>
    /// </remarks>
    public class CookieAuthentication : IAuthenticator
    {
        private readonly IAccountService _accountService;

        /// <summary>
        ///     Create a new instance of <see cref="CookieAuthentication" />.
        /// </summary>
        /// <param name="hashKey">Used to hash the ip address, recommended size is 64 bytes.</param>
        /// <param name="accountService">Used to load users</param>
        public CookieAuthentication(string hashKey, IAccountService accountService)
        {
            _accountService = accountService;
            CookieName = "GriffinAuth";
            AuthenticationScheme = "COOKIE";
            HashKey = hashKey;
        }

        /// <summary>
        ///     Name of the  authenticationCookie
        /// </summary>
        public string CookieName { get; set; }


        /// <summary>
        ///     TODO: why do we use an encoding and not Base64?
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        ///     Can be any length, but 64 bytes is recommended.
        /// </summary>
        /// <remarks>
        ///     Used to hash the machine identity
        /// </remarks>
        public string HashKey { get; set; }

        /// <summary>
        ///     Gets name of the authentication scheme
        /// </summary>
        /// <remarks>"BASIC", "DIGEST" etc.</remarks>
        public string AuthenticationScheme { get; private set; }

        /// <summary>
        ///     Create a WWW-Authenticate header
        /// </summary>
        public void CreateChallenge(IHttpRequest httpRequest, IHttpResponse response)
        {
        }

        /// <summary>
        ///     Authorize a request.
        /// </summary>
        /// <param name="request">Request being authenticated</param>
        /// <returns>UserName if successful; otherwise null.</returns>
        /// <exception cref="HttpException">403 Forbidden if the nonce is incorrect.</exception>
        public IAuthenticationUser Authenticate(IHttpRequest request)
        {
            var req = request as HttpRequest;
            if (req == null)
                return null;

            var cookie = req.Cookies[CookieName];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
                return null;

            var ourValue = Encode(req.RemoteEndPoint);
            var pos = cookie.Value.IndexOf(';');
            if (pos == -1)
                return null;

            if (ourValue != cookie.Value.Substring(0, pos))
                return null;

            var userName = cookie.Value.Substring(pos + 1);
            return _accountService.Lookup(userName, req.Uri);
        }

        /// <summary>
        ///     Create a new cookie
        /// </summary>
        /// <param name="remoteEndPoint">End point that want's an authentication cookie</param>
        /// <param name="userName">User name of the authenticated user</param>
        /// <param name="isPersistent">Store cookie for 60 days</param>
        /// <returns>
        ///     <c>HttpResponseCookie</c>
        /// </returns>
        public HttpResponseCookie CreateCookie(EndPoint remoteEndPoint, string userName, bool isPersistent)
        {
            return new HttpResponseCookie(CookieName, Encode(remoteEndPoint) + ";" + userName)
            {
                Path = "/",
                ExpiresUtc = isPersistent
                    ? DateTime.Today.AddDays(60)
                    : DateTime.MinValue
            };
        }

        /// <summary>
        ///     Hash end point
        /// </summary>
        /// <param name="remoteEndPoint">Endpoint to hash</param>
        /// <returns></returns>
        public string Encode(EndPoint remoteEndPoint)
        {
            var crypto = new HMACSHA256(Encoding.GetBytes(HashKey));
            var value = crypto.ComputeHash(Encoding.GetBytes(remoteEndPoint.ToString()));
            return Encoding.GetString(value);
        }
    }
}