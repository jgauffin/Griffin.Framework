using System;

namespace Griffin.Net.Protocols.Http.Authentication
{
    /// <summary>
    /// Uses a single realm for all requests.
    /// </summary>
    /// <example>
    /// <code>
    /// var digestAuthenticator = new DigestAuthenticator(new SingleRealmRepository("DragonsDen"), _myUserService);
    /// </code>
    /// </example>
    public class SingleRealmRepository : IRealmRepository
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleRealmRepository"/> class.
        /// </summary>
        /// <param name="name">Name of the realm.</param>
        public SingleRealmRepository(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
        }

        #region IRealmRepository Members

        /// <summary>
        /// Gets the realm for a request
        /// </summary>
        /// <param name="request">Request which realm we want to get</param>
        /// <returns>The realm that the request belongs to</returns>
        public string GetRealm(IHttpRequest request)
        {
            return _name;
        }

        #endregion
    }
}