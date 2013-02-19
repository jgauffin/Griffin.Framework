namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Status code for <see cref="IResponseAdapter"/>.
    /// </summary>
    public enum ResponseStatusCode
    {
        /// <summary>
        /// Authentication was successful, go ahead.
        /// </summary>
        Success,

        /// <summary>
        /// Incorrect authentication, try again
        /// </summary>
        Authenticate,

        /// <summary>
        /// Too many attempts, you may not proceed at all.
        /// </summary>
        Forbidden
    }
}