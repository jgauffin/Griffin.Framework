using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Authentication.Password.Strategies.Digest
{
    /*
    class DigestAuth
    {
        private readonly string _realm;

        public DigestAuth(string realm)
        {
            _realm = realm;
        }

        public void Check(Account account, string enteredPassword)
        {

            var uri = parameters["uri"];
            // Encode authentication info
            var ha1 = string.IsNullOrEmpty(account.PasswordOption)
                          ? GetHa1(_realm, account.AccountName, user.Password)
                          : user.HA1;

            // encode challenge info
            var a2 = String.Format("{0}:{1}", request.HttpMethod, uri);
            var ha2 = GetMd5HashBinHex(a2);
            var hashedDigest = Encrypt(ha1, ha2, parameters["qop"],
                                       parameters["nonce"], parameters["nc"], parameters["cnonce"]);

            //validate
            if (parameters["response"] == hashedDigest)
            {
                return user;
            }

            return null;            
        }

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
    }*/
}
