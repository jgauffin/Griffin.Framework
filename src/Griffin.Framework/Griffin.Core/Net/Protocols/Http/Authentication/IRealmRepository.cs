namespace Griffin.Net.Protocols.Http.Authentication
{
    /// <summary>
    /// Returns the realm for a request.
    /// </summary>
    /// <remarks>Realms are used during authentication</remarks>
    public interface IRealmRepository
    {
        /// <summary>
        /// Gets the realm for a request
        /// </summary>
        /// <param name="request">Request which realm we want to get</param>
        /// <returns>The realm that the request belongs to</returns>
        string GetRealm(IHttpRequest request);
    }
}