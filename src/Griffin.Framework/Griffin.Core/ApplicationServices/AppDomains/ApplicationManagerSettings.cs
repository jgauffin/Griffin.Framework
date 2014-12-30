using System;
using System.IO;

namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    ///     Settings for the app domain host.
    /// </summary>
    public class ApplicationManagerSettings
    {
        /// <summary>
        ///     Folder where new releases are placed.
        /// </summary>
        public string PickupPath { get; set; }

        /// <summary>
        ///     Base directory for new versions.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         New versions are put in sub directories to this folder.
        ///     </para>
        /// </remarks>
        public string AppDirectory { get; set; }

        /// <summary>
        ///     Name of the application
        /// </summary>
        /// <remarks>
        ///     <para>Used to generate registry settings.</para>
        /// </remarks>
        public string ApplicationName { get; set; }

        /// <summary>
        ///     Company/organization name
        /// </summary>
        /// <remarks>
        ///     <para>Used to generate registry settings.</para>
        /// </remarks>
        public string CompanyName { get; set; }


        /// <summary>
        ///     Validate settings.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Failed to read/write same value from registry.</exception>
        public void Validate()
        {
            if (!Directory.Exists(PickupPath))
                Directory.CreateDirectory(PickupPath);

            if (!Directory.Exists(AppDirectory))
                Directory.CreateDirectory(AppDirectory);

            var config = new RegistryConfigAdapter(CompanyName, ApplicationName);
            config["ConfigTest"] = "hello";
            if (config["ConfigTest"] != "hello")
                throw new InvalidOperationException("Failed to read/write same value from registry.");
        }
    }
}