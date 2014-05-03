using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Griffin.Core.Tests.Net
{
    public class Class1
    {
        [Fact]
        public void Test()
        {
            var cert =
                new X509Certificate2(AppDomain.CurrentDomain.BaseDirectory +
                                                    "\\Net\\cert\\GriffinNetworkingTemp.pfx", "mamma");

        }


    }
}
