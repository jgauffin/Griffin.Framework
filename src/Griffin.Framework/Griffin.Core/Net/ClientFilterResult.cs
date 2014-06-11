namespace Griffin.Net
{
    /// <summary>
    /// Result for <see cref="FilterMessageHandler"/>.
    /// </summary>
    public enum ClientFilterResult
    {
        /// <summary>
        /// Revoke message (do not handle it)
        /// </summary>
        Revoke,

        /// <summary>
        /// Accept message
        /// </summary>
        Accept
    }
}