namespace Griffin.Net.Channels
{
    /// <summary>
    /// current state of a channel
    /// </summary>
    public enum ChannelState
    {
        /// <summary>
        ///     Disconnecting socket
        /// </summary>
        Disconnecting,

        /// <summary>
        ///     Closing channel (trying to shut it down gracefully).
        /// </summary>
        Closing,

        /// <summary>
        /// Open and active
        /// </summary>
        Open,

        /// <summary>
        /// Closed
        /// </summary>
        Closed,

        /// <summary>
        /// Opening and not yet ready for IO operations
        /// </summary>
        Opening
    }
}