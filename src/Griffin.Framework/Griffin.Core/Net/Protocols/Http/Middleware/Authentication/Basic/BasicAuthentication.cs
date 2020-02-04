using System;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http.Authentication;

namespace Griffin.Net.Protocols.Http.Middleware.Authentication.Basic
{
    /// <summary>
    ///     Basic authentication
    /// </summary>
    public class BasicAuthentication : HttpMiddleware
    {
        private readonly string _realm;
        private readonly IAccountService _userService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BasicAuthentication" /> class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <param name="realm">The realm.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     userService
        ///     or
        ///     realm
        /// </exception>
        public BasicAuthentication(IAccountService userService, string realm)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _realm = realm ?? throw new ArgumentNullException(nameof(realm));
        }


        public override async Task Process(HttpContext context, Func<Task> next)
        {
            var authHeader = context.Request.Headers["Authorization"];
            if (authHeader == null || !authHeader.StartsWith("Basic "))
            {
                await next();
                if (context.Response.StatusCode == 401)
                {
                    CreateChallenge(context.Request, context.Response);
                }
                return;
            }

            authHeader = authHeader.Remove(0, "Basic ".Length);

            /*
             * To receive authorization, the client sends the userid and password,
                separated by a single colon (":") character, within a base64 [7]
                encoded string in the credentials.*/
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader));
            var pos = decoded.IndexOf(':');
            if (pos == -1)
            {
                throw new BadRequestException("Invalid basic authentication header, failed to find colon. Got: " +
                                              authHeader);
            }

            var password = decoded.Substring(pos + 1, decoded.Length - pos - 1);
            var userName = decoded.Substring(0, pos);

            var authContext = new AuthenticationContext
            {
                AuthenticationType = "basic",
                Secret = password,
                UserName = userName
            };
            var principal = await _userService.Authenticate(authContext);
            if (principal != null)
            {
                context.User = principal;
                await next();
                return;
            }

            var tries = (int) context.ChannelData.GetOrAdd("AuthenticationTries", s => 1);
            if (tries >= 5)
            {
                context.Response.StatusCode = 403;
                return;
            }

            context.ChannelData["AuthenticationTries"] = ++tries;
            CreateChallenge(context.Request, context.Response);
        }

        /// <summary>
        ///     Create a WWW-Authenticate header
        /// </summary>
        private void CreateChallenge(HttpRequest httpRequest, HttpResponse response)
        {
            response.AddHeader("WWW-Authenticate", "Basic realm=\"" + _realm + "\"");
            response.StatusCode = 401;
        }
    }
}