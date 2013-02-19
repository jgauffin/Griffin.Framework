using System;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Request used to authenticate the user
    /// </summary>
    /// <remarks>Some authentication techniques need additional information to able to authenticate, this interface is used to allow access to that information
    /// no matter if it's HTTP or a properitary protocol. Read their respective documentation
    /// to figure out what kind of values you have to supply.</remarks>
    /// <seealso cref="IRequestAdapter"/>
    public interface IRequestAdapter
    {
        /// <summary>
        /// Gets requested URL
        /// </summary>
        /// <remarks>For instance Digest authentication requires the URI to be able to calculate the hashes. the important thing
        /// is that this Uri remains the same for all future authentication requests (as when the user registered)</remarks>
        Uri Url { get; }

        /// <summary>
        /// Gets used method used when requesting information
        /// </summary>
        /// <remarks>HTTP Method for HTTP requests. Used when calculating hashes. Just leave it empty if not supported by your protocol</remarks>
        string Method { get; }

        /// <summary>
        /// Gets one of the headers
        /// </summary>
        /// <param name="name">Request a header value. Check the respective Authentication technique to see which headers that will be requested.</param>
        /// <returns>Value if found; otherwise <c>null</c>.</returns>
        string GetHeader(string name);
    }
}