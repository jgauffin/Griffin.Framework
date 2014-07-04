using System;
using System.IO;

namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationManagerSettings
    {
        public string PickupPath { get; set; }
        public string AppDirectory { get; set; }
        public string ApplicationName { get; set; }
        public string CompanyName { get; set; }

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