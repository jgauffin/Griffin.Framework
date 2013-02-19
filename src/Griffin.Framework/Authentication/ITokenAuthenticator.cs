using System.Security.Principal;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// TokenAuthenticator service
    /// </summary>
    public interface ITokenAuthenticator
    {
        /// <summary>
        /// Authenticate user 
        /// </summary>
        /// <param name="sourceName">Source, like "cookie", "apikey"</param>
        /// <param name="token">Authentication token (which was previously generated during a manual login)</param>
        /// <param name="remoteIp">IP address that the user login from</param>
        /// <returns>Principal to use (if authentication was successful); otherwise <c>false</c>.</returns>
        /// <remarks><para>
        /// The token must have a limited lifetime. 
        /// </para>
        /// <para>It's also better if the token is associated with a remote host, which increases security if someone has
        /// found the cookie.</para>
        /// </remarks>
        IPrincipal Login(string sourceName, string token, string remoteIp);
    }
}