using System;
using System.Security.Cryptography.X509Certificates;

namespace Griffin.Core.Tests.Net
{
    public class CertificateHelper
    {
        public static X509Certificate2 Create()
        {
            return new X509Certificate2(
                AppDomain.CurrentDomain.BaseDirectory + "\\Net\\cert\\GriffinNetworkingTemp.pfx", "mamma");
        }
    }
}