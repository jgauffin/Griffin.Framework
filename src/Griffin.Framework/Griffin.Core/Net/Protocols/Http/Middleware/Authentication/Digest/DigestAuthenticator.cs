using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Griffin.Logging;
using Griffin.Net.Protocols.Http.Authentication;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http.Middleware.Authentication.Digest
{
    /// <summary>
    ///     Implements Digest authentication.
    /// </summary>
    /// <remarks>Read RFC 2617 for more information</remarks>
    public class DigestAuthenticator : HttpMiddleware
    {
        /// <summary>
        ///     Used by test classes to be able to use hardcoded values
        /// </summary>
        public static bool DisableNonceCheck = true;

        private readonly ILogger _logger = LogManager.GetLogger<DigestAuthenticator>();
        private readonly NonceService _nonceService = new NonceService();
        private readonly IRealmRepository _realmRepository;
        private readonly IAccountService _userService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DigestAuthenticator" /> class.
        /// </summary>
        /// <param name="realmRepository">Used to lookup the realm for a HTTP request</param>
        /// <param name="userService">Supplies users during authentication process.</param>
        public DigestAuthenticator(IRealmRepository realmRepository, IAccountService userService)
        {
            _realmRepository = realmRepository;
            _userService = userService;
        }

        /// <summary>
        ///     Generate a HA1 hash
        /// </summary>
        /// <param name="realm">Realm that the user want to authenticate in</param>
        /// <param name="userName">USername</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public static string GetHa1(string realm, string userName, string password)
        {
            return GetMd5HashBinHex($"{userName}:{realm}:{password}");
        }

        /// <summary>
        ///     Gets the Md5 hash bin hex2.
        /// </summary>
        /// <param name="toBeHashed">To be hashed.</param>
        /// <returns></returns>
        public static string GetMd5HashBinHex(string toBeHashed)
        {
            var hashAlgorithm = MD5.Create();
            var hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(toBeHashed));
            return string.Concat(hash.Select(b => b.ToString("x2")).ToArray());
        }

        public override async Task Process(HttpContext context, Func<Task> next)
        {
            var request = context.Request;
            var authHeader = request.Headers["Authorization"];
            if (authHeader == null || !authHeader.StartsWith("digest ", StringComparison.OrdinalIgnoreCase))
            {
                await next();

                if (context.Response.StatusCode == 401 && !context.Response.Headers.Contains("WWW-Authenticate"))
                    CreateChallenge(context.Request, context.Response);

                return;
            }


            var parameters = ParameterCollection.Parse(authHeader.Remove(0, "Digest ".Length));

            var nc = int.Parse(parameters["nc"], NumberStyles.AllowHexSpecifier);
            if (!_nonceService.IsValid(parameters["nonce"], nc) && !DisableNonceCheck)
                throw new HttpException(HttpStatusCode.Forbidden, "Invalid nonce/nc.");

            // request authentication information
            var username = parameters["username"];
            var uri = parameters["uri"];

            var authContext = new AuthenticationContext
            {
                UserName = username,
                AuthenticationType = "digest",
                CompareHashFunc = hashingContext =>
                {
                    var ha1 = string.IsNullOrEmpty(hashingContext.HashedPassword)
                        ? GetHa1(_realmRepository.GetRealm(request), username, hashingContext.Password)
                        : hashingContext.HashedPassword;

                    var a2 = $"{request.HttpMethod}:{uri}";
                    var ha2 = GetMd5HashBinHex(a2);
                    var hashedDigest = Encrypt(ha1, ha2, parameters["qop"],
                        parameters["nonce"], parameters["nc"], parameters["cnonce"]);

                    return hashedDigest == parameters["response"];
                }
            };

            var principal = await _userService.Authenticate(authContext);
            if (principal != null)
            {
                context.User = principal;
                await next();
                return;
            }

            var tries = (int)context.ChannelData.GetOrAdd("AuthenticationTries", s => 1);
            if (tries >= 5)
            {
                context.Response.StatusCode = 403;
                return;
            }

            context.ChannelData["AuthenticationTries"] = ++tries;
            CreateChallenge(context.Request, context.Response);
        }

        /// <summary>
        /// </summary>
        /// <param name="ha1">Md5 hex encoded "userName:realm:password", without the quotes.</param>
        /// <param name="ha2">Md5 hex encoded "method:uri", without the quotes</param>
        /// <param name="qop">Quality of Protection</param>
        /// <param name="nonce">"Number used ONCE"</param>
        /// <param name="nc">Hexadecimal request counter.</param>
        /// <param name="cnonce">Client number used once</param>
        /// <returns></returns>
        protected virtual string Encrypt(string ha1, string ha2, string qop, string nonce, string nc, string cnonce)
        {
            string unhashedDigest;
            if (qop == "auth-int" || qop == "auth")
                unhashedDigest = $"{ha1}:{nonce}:{nc}:{cnonce}:{qop}:{ha2}";
            else
                unhashedDigest = $"{ha1}:{nonce}:{ha2}";

            return GetMd5HashBinHex(unhashedDigest);
        }

        private void CreateChallenge(HttpRequest request, HttpResponse response)
        {
            var nonce = _nonceService.CreateNonce();

            var challenge = new StringBuilder();
            challenge.AppendFormat(@"Digest realm=""{0}"", ", _realmRepository.GetRealm(request));
            challenge.AppendFormat(@"nonce=""{0}"", ", nonce);
            challenge.Append(@"qop=""auth"", ");
            challenge.Append("algorithm=MD5");


            /* RFC 2617 3.3
                Because the client is required to return the value of the opaque
               directive given to it by the server for the duration of a session,
               the opaque data may be used to transport authentication session state
               information. (Note that any such use can also be accomplished more
               easily and safely by including the state in the nonce.) For example,
               a server could be responsible for authenticating content that
               actually sits on another server. It would achieve this by having the
               first 401 response include a domain directive whose value includes a
               URI on the second server, and an opaque directive whose value            
               contains the state information. The client will retry the request, at
               which time the server might respond with a 301/302 redirection,
               pointing to the URI on the second server. The client will follow the
               redirection, and pass an Authorization header , including the
               <opaque> data.
            */
            // , opaque=""" + Guid.NewGuid().ToString().Replace("-", string.Empty) + "\""


            /* Disable the stale mechanism
             * We should really generate the responses directly in these classes.
            challenge.Append(", stale=");
            challenge.Append((bool)options[0] ? "true" : "false");
            challenge.Append("false");
             * */


            response.AddHeader("WWW-Authenticate", challenge.ToString());
            response.StatusCode = 401;
        }
    }
}