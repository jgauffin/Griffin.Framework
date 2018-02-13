#if NET451
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Configuration
{
    public class ConfigurationManagerReader : IConfigurationReader
    {
        public string ReadAppSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

        public string ReadValue(string sectionName, string propertyName)
        {
            var section =  ConfigurationManager.GetSection(sectionName) as AppSettingsSection;
            if (section == null)
                throw new ConfigurationErrorsException("Section " + sectionName + " is not of type AppSettingsSection");

            return section.Settings[propertyName].Value;
        }
    }
}
#endif