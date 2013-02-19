using System;

namespace Griffin.Framework.Authentication.Password
{
    /// <summary>
    /// Used to be able to supply information which is required for the different strategies.
    /// </summary>
    public class ComparePasswordContext
    {
        public ComparePasswordContext(string storedPassword, string clearTextPassword, string extraParameter)
        {
            if (storedPassword == null) throw new ArgumentNullException("storedPassword");
            if (clearTextPassword == null) throw new ArgumentNullException("clearTextPassword");
            ExtraParameter = extraParameter;
            ClearTextPassword = clearTextPassword;
            StoredPassword = storedPassword;
        }

        /// <summary>
        /// Gets password stored in the data source
        /// </summary>
        public string StoredPassword { get; private set; }

        /// <summary>
        /// Gets password as entered during the login attempt
        /// </summary>
        public string ClearTextPassword { get; private set; }

        /// <summary>
        /// Gets extra parameter.
        /// </summary>
        /// <remarks>The value of this parameter depends on the currently used password strategy.</remarks>
        public string ExtraParameter { get; private set; }
    }
}