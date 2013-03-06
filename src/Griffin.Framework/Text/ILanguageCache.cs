namespace Griffin.Framework.Text
{
    /// <summary>
    /// Allows you to store all language prompts somewhere for faster access.
    /// </summary>
    /// <remarks>Hitting a DB or similar slow store might not be the best
    /// thing when working with localization. You can therefore use this interface to
    /// create a cache somewhere.</remarks>
    public interface ILanguageCache
    {
        /// <summary>
        /// Get a language string
        /// </summary>
        /// <param name="languageCode">Language code such as "en-us" or just "en"</param>
        /// <param name="key">Text key. May be long (for instance a complete paragraph)</param>
        /// <returns>Text if found; otherwise <c>null</c>.</returns>
        string Get(string languageCode, string key);

        /// <summary>
        /// Add a new string to the cache
        /// </summary>
        /// <param name="languageCode">Language code such as "en-us" or just "en"</param>
        /// <param name="key">Text key. Might be long (for instance a complete paragraph)</param>
        /// <param name="text">Text to add</param>
        /// <remarks>Note to implementors: For fast access you can convert the key to a hash (but don't forget to handle hash collisions)</remarks>
        void Add(string languageCode, string key, string text);
    }
}