using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Griffin.Authentication.Password.Strategies.Digest
{
    /*
    /// <summary>
    /// Implements Digest authentication.
    /// </summary>
    /// <remarks>
    /// <para>Sets the "WWW-Authenticate" header in the response to trigger authentication.</para>
    /// <para>Read RFC 2617 for more information</para>
    /// </remarks>
    public class DigestAuthenticator
    {
        private readonly string _realm;
        private static NonceService _nonceService = new NonceService();

        /// <summary>
        /// Used by test classes to be able to use hardcoded values
        /// </summary>
        public static bool DisableNonceCheck = true;

        public DigestAuthenticator(string realm)
        {
            _realm = realm;
        }

        /// <summary>
        /// Create a WWW-Authenticate header
        /// </summary>
        public void CreateChallenge(IRequestAdapter request, IResponseAdapter response)
        {
            var nonce = _nonceService.CreateNonce();

            var challenge = new StringBuilder();
            challenge.AppendFormat(@"Digest realm=""{0}"", ", _realm);
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
             * * /


            response.AddHeader("WWW-Authenticate", challenge.ToString());
            response.SetStatus(ResponseStatusCode.Authenticate, "You need to authenticate");
        }

        /// <summary>
        /// Gets name of the authentication scheme
        /// </summary>
        /// <remarks>"BASIC", "DIGEST" etc.</remarks>
        public string AuthenticationScheme
        {
            get { return "digest"; }
        }

        public ResponseStatusCode Authenticate(IRequestAdapter request)
        {
            var authHeader = request.GetHeader("Authorization");
            if (authHeader == null)
                return ResponseStatusCode.Authenticate;

            var parameters = Regex.Matches(authHeader, @"(?<name>\w+)=(""(?<val>[^""]*)""|(?<val>[^"" ,\t\r\n]+))")
                .Cast<Match>()
                .ToDictionary(m => m.Groups["name"].Value, m => m.Groups["val"].Value);

            var nc = int.Parse(parameters["nc"], NumberStyles.AllowHexSpecifier);
            if (!_nonceService.IsValid(parameters["nonce"], nc) && !DisableNonceCheck)
                return ResponseStatusCode.Forbidden;

            // request authentication information
            var username = parameters["username"];
            var context = new GetAccountContext("DIGEST", username, null, request.Url);
            var user = _userRepository.Get(context);
            if (user == null)
                return null;

        }

    }
    */
}