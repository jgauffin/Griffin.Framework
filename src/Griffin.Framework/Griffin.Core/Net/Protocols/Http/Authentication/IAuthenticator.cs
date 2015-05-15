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
        /// <returns>User if successful; otherwise null.</returns>
        /// <exception cref="HttpException">Http exceptions will generate a response. Use for instance 403 Forbidden if the user may not connect.</exception>
        IAuthenticationUser Authenticate(IHttpRequest request);
    }
}