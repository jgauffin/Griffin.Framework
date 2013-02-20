using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Framework.Text
{
    /// <summary>
    /// Gets localization resources from a string table.
    /// </summary>
    public class StringTableProvider : ITextDataSource
    {
        private readonly ResourceManager _resourceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringTableProvider" /> class.
        /// </summary>
        /// <param name="resourceManager">The resource manager.</param>
        public StringTableProvider(ResourceManager resourceManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");
            _resourceManager = resourceManager;
        }

        /// <summary>
        /// Get translation
        /// </summary>
        /// <param name="identifier">Identifier. Might be long. You have to hash yourself (and manage hash collisions)</param>
        /// <returns>null = do not exist; "" = not translated; any other value should be the correct text.</returns>
        public string Get(string identifier)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");
            return _resourceManager.GetString(identifier);
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="identifier">Identifier. Might be long. You have to hash yourself (and manage hash collisions)</param>
        /// <param name="metadataName">Name of the meta.</param>
        /// <returns>null = do not exist; "" = not translated; any other value should be the correct text.</returns>
        public string Get(string identifier, string metadataName)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");
            if (metadataName == null) throw new ArgumentNullException("metadataName");
            return _resourceManager.GetString(identifier + "_");
        }
    }
}
