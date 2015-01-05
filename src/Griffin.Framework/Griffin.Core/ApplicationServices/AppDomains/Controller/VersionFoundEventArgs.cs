using System;

namespace Griffin.ApplicationServices.AppDomains.Controller
{
    /// <summary>
    ///     Version that was found.
    /// </summary>
    public class VersionFoundEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionFoundEventArgs"/> class.
        /// </summary>
        /// <param name="versionPath">The version path.</param>
        /// <exception cref="System.ArgumentNullException">versionPath</exception>
        public VersionFoundEventArgs(string versionPath)
        {
            if (versionPath == null) throw new ArgumentNullException("versionPath");
            VersionPath = versionPath;
        }

        /// <summary>
        ///     Full path to the version directory (that the <c>NewVersionDetector</c> created after discovering new binaries).
        /// </summary>
        /// <remarks>
        ///     The specified path is the one that should be started within a new <c>AppDomain</c>.
        /// </remarks>
        public string VersionPath { get; set; }
    }
}