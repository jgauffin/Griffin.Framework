namespace Griffin.Net.Protocols.Http.Authentication
{
    /// <summary>
    /// Used to authenticate the user.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Gets name of the authentication scheme
        /// </summary>
        /// <remarks>"BASIC", "DIGEST" etc.</remarks>
        string AuthenticationScheme { get; }

        /// <summary>
        /// Create a WWW-Authenticate header
        /// </summary>
        void CreateChallenge(IHttpRequest httpRequest, IHttpResponse response);

        /// <summary>
        /// Authorize a request.
        /// </summary>
        /// <param name="request">Request being authenticated</param>
        /// <returns>UserName if successful; otherwise null.</returns>
        /// <exception cref="HttpException">403 Forbidden if the nonce is incorrect.</exception>
        string Authenticate(IHttpRequest request);
    }
}