using System;
using Microsoft.Win32;

namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    /// </summary>
    public class RegistryConfigAdapter : IConfigAdapter
    {
        private readonly string _appName;
        private readonly string _companyName;

        public RegistryConfigAdapter(string companyName, string appName)
        {
            _companyName = companyName;
            _appName = appName;
        }

        public string this[string name]
        {
            get { return GetStringValue(name); }
            set { SetValue(name, value); }
        }

        public string GetStringValue(string name)
        {
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

        public void SetValue(string name, object value)
        {
            RegistryKey appKey, companyKey;
            OpenKey(true, out companyKey, out appKey);
            try
            {
                appKey.SetValue(name, value);
            }
            finally
            {
                appKey.Close();
                companyKey.Close();
            }
        }

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