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
                X509Certificate.CreateFromCertFile(AppDomain.CurrentDomain.BaseDirectory +
                                                    "\\cert\\GriffinNetworking.cer");

        }


    }
}
