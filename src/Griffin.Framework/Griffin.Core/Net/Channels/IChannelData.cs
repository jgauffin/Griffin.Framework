namespace Griffin.Net.Channels
{
    /// <summary>
    /// Used to store data in a connected channel.
    /// </summary>
    public interface IChannelData
    {
        /// <summary>
        /// Get or set data
        /// </summary>
        /// <param name="key">Identifier (note that everyone with access to the channel can access the data, use careful naming)</param>
        /// <returns>Data if found; otherwise <c>null</c>.</returns>
        object this[string key] { get; set; }
    }
}