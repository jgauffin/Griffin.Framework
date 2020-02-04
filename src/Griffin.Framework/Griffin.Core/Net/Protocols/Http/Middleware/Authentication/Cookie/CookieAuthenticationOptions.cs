using System;
using System.Text;

namespace Griffin.Net.Protocols.Http.Middleware.Authentication.Cookie
{
    public class CookieAuthenticationOptions
    {
        /// <summary>
        ///     Name of the  authenticationCookie
        /// </summary>
        public string CookieName { get; set; } = "GriffinAuth";


        /// <summary>
        ///     TODO: why do we use an encoding and not Base64?
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        ///     Can be any length, but 64 bytes is recommended.
        /// </summary>
        /// <remarks>
        ///     Used to hash the machine identity
        /// </remarks>
        public string HashKey { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        ///     Gets name of the authentication scheme
        /// </summary>
        /// <remarks>"BASIC", "DIGEST" etc.</remarks>
        public string AuthenticationScheme { get; set; } = "Cookie";

        public bool IsPersistent { get; set; }
    }
}