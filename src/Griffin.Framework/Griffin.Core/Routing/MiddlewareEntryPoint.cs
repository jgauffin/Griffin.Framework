namespace Griffin.Routing
{
    /// <summary>
    /// Middleware entry point.
    /// </summary>
    public enum MiddlewareEntryPoint
    {
        /// <summary>
        /// The authentication entry point is the first one.
        /// </summary>
        Authentication = 0,
        /// <summary>
        /// The prerouting entry point will be called after authentication is done.
        /// </summary>
        PreRouting,
        /// <summary>
        /// The postrouting entry point is right after the routing.
        /// </summary>
        PostRouting,
        /// <summary>
        /// The postresponse entry point is the last entry point and will be called after the response is sent.
        /// </summary>
        PostResponse
    }
}
