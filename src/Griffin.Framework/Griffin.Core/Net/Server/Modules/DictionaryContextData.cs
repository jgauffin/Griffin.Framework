using System.Collections.Generic;

namespace Griffin.Cqs.Net.Modules
{
    /// <summary>
    /// Uses a dictionary to store information
    /// </summary>
    public class DictionaryContextData : IContextData
    {
        private readonly IDictionary<string, object> _items = new Dictionary<string, object>();

        /// <summary>
        /// Get or set information specific for the current message processing
        /// </summary>
        /// <param name="name">Name of property</param>
        /// <returns>value if found; otherwise <c>null</c>.</returns>
        public object this[string name]
        {
            get
            {
                object item;
                return _items.TryGetValue(name, out item) ? item : null;
            }
            set
            {
                _items[name] = value;
            }
        }
    }
}