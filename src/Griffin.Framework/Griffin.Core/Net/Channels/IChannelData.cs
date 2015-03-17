using System;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// Used to store data in a connected channel.
    /// </summary>
    public interface IChannelData
    {
        /// <summary>
        /// Get or add a value
        /// </summary>
        /// <param name="key">key to get</param>
        /// <param name="addCallback">Should return value to add if the key is not found</param>
        object GetOrAdd(string key, Func<string, object> addCallback);

        /// <summary>
        /// Try updating a value
        /// </summary>
        /// <param name="key">Key for the value to update</param>
        /// <param name="newValue">Value to set</param>
        /// <param name="existingValue">Value that we've previously retrieved</param>
        /// <returns><c>true</c> if the existing value is the same as the one in the dictionary</returns>
        bool TryUpdate(string key, object newValue, object existingValue);

        /// <summary>
        /// Get or set data
        /// </summary>
        /// <param name="key">Identifier (note that everyone with access to the channel can access the data, use careful naming)</param>
        /// <returns>Data if found; otherwise <c>null</c>.</returns>
        object this[string key] { get; set; }

        /// <summary>
        /// Remove all existing data.
        /// </summary>
        void Clear();
    }
}