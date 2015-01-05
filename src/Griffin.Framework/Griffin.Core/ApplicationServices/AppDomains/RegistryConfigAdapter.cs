using System;
using Microsoft.Win32;

namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    /// Used to keep configuration settings in the registry (in CURRENT_USER).
    /// </summary>
    public class RegistryConfigAdapter : IConfigAdapter
    {
        private readonly string _appName;
        private readonly string _companyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryConfigAdapter"/> class.
        /// </summary>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="appName">Name of the application.</param>
        /// <exception cref="System.ArgumentNullException">
        /// companyName
        /// or
        /// appName
        /// </exception>
        public RegistryConfigAdapter(string companyName, string appName)
        {
            if (companyName == null) throw new ArgumentNullException("companyName");
            if (appName == null) throw new ArgumentNullException("appName");
            _companyName = companyName;
            _appName = appName;
        }

        /// <summary>
        /// Get a settings value
        /// </summary>
        /// <param name="name">Name of the setting.</param>
        /// <returns>value if found; otherwise <c>null</c>.</returns>
        public string this[string name]
        {
            get { return GetStringValue(name); }
            set { SetValue(name, value); }
        }

        /// <summary>
        /// Get a value
        /// </summary>
        /// <param name="name">setting name</param>
        /// <returns>
        /// value if found; otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">name</exception>
        public string GetStringValue(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            RegistryKey appKey, companyKey;
            OpenKey(false, out companyKey, out appKey);
            try
            {
                var value = appKey.GetValue(name);
                return value != null ? value.ToString() : null;
            }
            finally
            {
                appKey.Close();
                companyKey.Close();
            }
        }

        /// <summary>
        /// Set a value
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <param name="value"><c>null</c> to remove value</param>
        public void SetValue(string name, object value)
        {
            if (name == null) throw new ArgumentNullException("name");
            RegistryKey appKey, companyKey;
            OpenKey(true, out companyKey, out appKey);
            try
            {
                if (value == null)
                    appKey.DeleteValue(name);
                else
                    appKey.SetValue(name, value);
            }
            finally
            {
                appKey.Close();
                companyKey.Close();
            }
        }

        /// <summary>
        /// Opens the key.
        /// </summary>
        /// <param name="writable">if set to <c>true</c>, the registry setting should be opened as writable.</param>
        /// <param name="companyKey">The company name.</param>
        /// <param name="appKey">The application name.</param>
        /// <exception cref="System.InvalidOperationException">Failed to create ' + _companyName + ' under CURRENT_USER.</exception>
        protected virtual void OpenKey(bool writable, out RegistryKey companyKey, out RegistryKey appKey)
        {
            companyKey = Registry.CurrentUser.OpenSubKey(_companyName, writable)
                         ?? Registry.CurrentUser.CreateSubKey(_companyName);
            if (companyKey == null)
                throw new InvalidOperationException("Failed to create '" + _companyName + "' under CURRENT_USER.");

            appKey = companyKey.OpenSubKey(_appName, writable)
                     ?? companyKey.CreateSubKey(_appName);
        }
    }
}