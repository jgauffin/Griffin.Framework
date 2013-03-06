using System;
using System.Collections.Concurrent;

namespace Griffin.Framework.Text
{
    /// <summary>
    /// Uses a <c>ConcurrentDictionary</c> to store all languages.
    /// </summary>
    public class MemoryLanguageCache : ILanguageCache
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _languageCache
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        /// <summary>
        /// Get a language string
        /// </summary>
        /// <param name="languageCode">Language code such as "en-us" or just "en"</param>
        /// <param name="key">Text key. May be long (for instance a complete paragraph)</param>
        /// <returns>
        /// Text if found; otherwise <c>null</c>.
        /// </returns>
        public string Get(string languageCode, string key)
        {
            ConcurrentDictionary<string, string> lang;

            if (!_languageCache.TryGetValue(languageCode, out lang))
                return null;

            string text;
            return lang.TryGetValue(key, out text) ? text : null;
        }

        /// <summary>
        /// Add a new string to the cache
        /// </summary>
        /// <param name="languageCode">Language code such as "en-us" or just "en"</param>
        /// <param name="key">Text key. Might be long (for instance a complete paragraph)</param>
        /// <param name="text">Text to add</param>
        /// <exception cref="System.InvalidOperationException">Failed to insert language  + languageCode</exception>
        /// <remarks>
        /// Note to implementors: For fast access you can convert the key to a hash (but don't forget to handle hash collisions)
        /// </remarks>
        public void Add(string languageCode, string key, string text)
        {
            ConcurrentDictionary<string, string> lang;
            if (!_languageCache.TryGetValue(languageCode, out lang))
            {
                _languageCache.AddOrUpdate(languageCode,
                                           new ConcurrentDictionary<string, string>(),
                                           (k, value) => value);
                if (!_languageCache.TryGetValue(languageCode, out lang))
                    throw new InvalidOperationException("Failed to insert language " + languageCode);
            }


            lang.AddOrUpdate(key, text, (k, v) => text);
        }
    }
}