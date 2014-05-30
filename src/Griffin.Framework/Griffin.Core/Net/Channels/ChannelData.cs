using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// Uses a concurrent dictionary to store all itemns.
    /// </summary>
    public class ChannelData : IChannelData
    {
        private readonly ConcurrentDictionary<string, object> _items = new ConcurrentDictionary<string, object>();

        public object GetOrAdd(string key, Func<string, object> addCallback)
        {
            return _items.GetOrAdd(key, addCallback);
        }

        /// <summary>
        /// Try updating a value
        /// </summary>
        /// <param name="key">Key for the value to update</param>
        /// <param name="newValue">Value to set</param>
        /// <param name="existingValue">Value that we've previously retreived</param>
        /// <returns><c>true</c> if the existing value is the same as the one in the dictionary</returns>
        public bool TryUpdate(string key, object newValue, object existingValue)
        {
            return _items.TryUpdate(key, newValue, existingValue);
        }

        /// <summary>
        /// Get or set data
        /// </summary>
        /// <param name="key">Identifier (note that everyone with access to the channel can access the data, use careful naming)</param>
        /// <returns>Data if found; otherwise <c>null</c>.</returns>
        public object this[string key]
        {
            get
            {
                return _items[key];
            }
            set
            {
                _items[key] = value;
            }
        }

        /// <summary>
        /// Remove all existing data.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }
    }
}
