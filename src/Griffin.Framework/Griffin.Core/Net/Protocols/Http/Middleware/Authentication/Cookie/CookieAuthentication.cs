using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http.Authentication;

namespace Griffin.Net.Protocols.Http.Middleware.Authentication.Cookie
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
    public class CookieAuthentication : HttpMiddleware
    {
        private readonly CookieAuthenticationOptions _options;
        private readonly IAccountService _accountService;
        private HMACSHA256 _crypto;

        /// <summary>
        ///     Create a new instance of <see cref="CookieAuthentication" />.
        /// </summary>
        /// <param name="options">Cookie settings.</param>
        /// <param name="accountService">Used to load users</param>
        public CookieAuthentication(CookieAuthenticationOptions options, IAccountService accountService)
        {
            _options = options;
            _accountService = accountService;
            _crypto = new HMACSHA256(_options.Encoding.GetBytes(_options.HashKey));
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
            return new HttpResponseCookie(_options.CookieName, $"{Encode(remoteEndPoint)};{userName}")
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
        private string Encode(EndPoint remoteEndPoint)
        {
            var value = _crypto.ComputeHash(_options.Encoding.GetBytes(remoteEndPoint.ToString()));
            return _options.Encoding.GetString(value);
        }

        public override async Task Process(HttpContext context, Func<Task> next)
        {
            var cookie = context.Request.Cookies[_options.CookieName];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                if (context.User != null)
                {
                    var newCookie = CreateCookie(
                        context.Request.RemoteEndPoint,
                        context.User.Identity.Name,
                        _options.IsPersistent);
                    context.Response.Cookies.Add(newCookie);
                }

                return;
            }

            var pos = cookie.Value.IndexOf(';');
            if (pos == -1)
                return;

            var ourValue = Encode(context.Request.RemoteEndPoint);
            if (ourValue != cookie.Value.Substring(0, pos))
                return;

            var userName = cookie.Value.Substring(pos + 1);
            var account = await _accountService.LoadAsync(context.Request.Uri, userName);
            context.User = account;
        }
    }
}