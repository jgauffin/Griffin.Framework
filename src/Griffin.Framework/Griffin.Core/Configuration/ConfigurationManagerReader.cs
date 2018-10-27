#if NET451 || NET45
using System.Collections.Specialized;
using System.Configuration;

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
            if (section != null)
                return section.Settings[propertyName].Value;

            var section2 =  ConfigurationManager.GetSection(sectionName) as NameValueCollection;
            if (section2 != null)
                return section2[propertyName];
                
    
            throw new ConfigurationErrorsException("Section " + sectionName + " is not of type AppSettingsSection");
        }
    }
}
#endif