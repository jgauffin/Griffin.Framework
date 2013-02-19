using System;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Used to associate an <see cref="Account"/> with external login services.
    /// </summary>
    public class ExternalId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalId" /> class.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerId">The provider id.</param>
        /// <param name="category"><see cref="Category"/>.</param>
        /// <exception cref="System.ArgumentNullException">providerName</exception>
        public ExternalId(string providerName, string providerId, string category)
        {
            if (providerName == null) throw new ArgumentNullException("providerName");
            if (providerId == null) throw new ArgumentNullException("providerId");

            ProviderName = providerName;
            ProviderUserId = providerId;
            Category = category;
        }

        
        /// <summary>
        /// Gets provider which supplies this association
        /// </summary>
        /// <remarks>For OpenAuth this may be "facebook" while for token based authentication this might just be "token"</remarks>
        public string ProviderName { get; private set; }

        /// <summary>
        /// Gets identitfy for the external authentication mechanism
        /// </summary>
        public string ProviderUserId { get; private set; }

        /// <summary>
        /// Category is used by authentication controllers to indicate which type of authentication this is
        /// </summary>
        /// <remarks>For instance the OpenAuth integration sets this field to "OpenAuth" to be able to find
        /// all associated open auth providers.</remarks>
        public string Category { get; set; }
    }
}