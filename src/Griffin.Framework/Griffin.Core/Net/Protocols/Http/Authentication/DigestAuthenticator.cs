using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Griffin.Logging;
using Griffin.Net.Protocols.Http.Authentication.Digest;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http.Authentication
{
    /// <summary>
    /// Implements Digest authentication.
    /// </summary>
    /// <remarks>Read RFC 2617 for more information</remarks>
    public class DigestAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Used by test classes to be able to use hardcoded values
        /// </summary>
        public static bool DisableNonceCheck = true;

        private readonly ILogger _logger = LogManager.GetLogger<DigestAuthenticator>();
        private readonly NonceService _nonceService = new NonceService();
        private readonly IRealmRepository _realmRepository;
        private readonly IAccountService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigestAuthenticator"/> class.
        /// </summary>
        /// <param name="realmRepository">Used to lookup the realm for a HTTP request</param>
        /// <param name="userService">Supplies users during authentication process.</param>
        public DigestAuthenticator(IRealmRepository realmRepository, IAccountService userService)
        {
            _realmRepository = realmRepository;
            _userService = userService;
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
            get { return "digest"; }
        }

        #region IAuthenticator Members

        /// <summary>
        /// Create a WWW-Authenticate header
        /// </summary>
        public void CreateChallenge(IHttpRequest request, IHttpResponse response)
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

        /// <summary>
        /// Gets name of the authentication scheme
        /// </summary>
        /// <remarks>"BASIC", "DIGEST" etc.</remarks>
        public string AuthenticationScheme
        {
            get { return "digest"; }
        }

        public string Authenticate(IHttpRequest request)
        {
            var authHeader = request.Headers["Authorization"];
            if (authHeader == null)
                return null;


            var parameters = ParameterCollection.Parse(authHeader.Remove(0, AuthenticationScheme.Length + 1));

            var nc = int.Parse(parameters["nc"], NumberStyles.AllowHexSpecifier);
            if (!_nonceService.IsValid(parameters["nonce"], nc) && !DisableNonceCheck)
                throw new HttpException(HttpStatusCode.Forbidden, "Invalid nonce/nc.");

            // request authentication information
            var username = parameters["username"];
            var user = _userService.Lookup(username, request.Uri);
            if (user == null)
                return null;

            var uri = parameters["uri"];
            // Encode authentication info
            var ha1 = string.IsNullOrEmpty(user.HA1)
                          ? GetHa1(_realmRepository.GetRealm(request), username, user.Password)
                          : user.HA1;

            // encode challenge info
            var a2 = String.Format("{0}:{1}", request.HttpMethod, uri);
            var ha2 = GetMd5HashBinHex(a2);
            var hashedDigest = Encrypt(ha1, ha2, parameters["qop"],
                                       parameters["nonce"], parameters["nc"], parameters["cnonce"]);

            //validate
            if (parameters["response"] == hashedDigest)
            {
                return user.Username;
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Encrypts parameters into a Digest string
        /// </summary>
        /// <param name="realm">Realm that the user want to log into.</param>
        /// <param name="userName">User logging in</param>
        /// <param name="password">Users password.</param>
        /// <param name="method">HTTP method.</param>
        /// <param name="uri">Uri/domain that generated the login prompt.</param>
        /// <param name="qop">Quality of Protection.</param>
        /// <param name="nonce">"Number used ONCE"</param>
        /// <param name="nc">Hexadecimal request counter.</param>
        /// <param name="cnonce">"Client Number used ONCE"</param>
        /// <returns>Digest encrypted string</returns>
        public static string Encrypt(string realm, string userName, string password, string method, string uri,
                                     string qop, string nonce, string nc, string cnonce)
        {
            var ha1 = GetHa1(realm, userName, password);
            var a2 = String.Format("{0}:{1}", method, uri);
            var ha2 = GetMd5HashBinHex(a2);

            string unhashedDigest;
            if (qop != null)
            {
                unhashedDigest = String.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                               ha1,
                                               nonce,
                                               nc,
                                               cnonce,
                                               qop,
                                               ha2);
            }
            else
            {
                unhashedDigest = String.Format("{0}:{1}:{2}",
                                               ha1,
                                               nonce,
                                               ha2);
            }

            return GetMd5HashBinHex(unhashedDigest);
        }

        public static string GetHa1(string realm, string userName, string password)
        {
            return GetMd5HashBinHex(String.Format("{0}:{1}:{2}", userName, realm, password));
        }

        /// <summary>
        /// 
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
            {
                unhashedDigest = String.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                               ha1,
                                               nonce,
                                               nc,
                                               cnonce,
                                               qop,
                                               ha2);
            }
            else
            {
                unhashedDigest = String.Format("{0}:{1}:{2}",
                                               ha1,
                                               nonce,
                                               ha2);
            }

            return GetMd5HashBinHex(unhashedDigest);
        }

        /// <summary>
        /// Gets the Md5 hash bin hex2.
        /// </summary>
        /// <param name="toBeHashed">To be hashed.</param>
        /// <returns></returns>
        public static string GetMd5HashBinHex(string toBeHashed)
        {
            var hashAlgorithm = MD5.Create();
            var hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(toBeHashed));
            return string.Concat(hash.Select(b => b.ToString("x2")).ToArray());
        }
    }
}
