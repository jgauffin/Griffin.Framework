using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Authentication;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Http.Authentication
{
    public class DigestAuthenticatorTests
    {
        /// <summary>
        /// Values are taken from the Wikipedia Article
        /// </summary>
        [Fact]
        public void Test()
        {
            var uri = new Uri("http://testrealm@host.com/dir/index.html");
            var headerValue =
                @"Digest username=""Mufasa"", realm=""testrealm@host.com"", nonce=""dcd98b7102dd2f0e8b11d0f600bfb0c093"", uri=""/dir/index.html"", qop=auth, nc=00000001, cnonce=""0a4f113b"", response=""6629fae49393a05397450978507c4ef1"", opaque=""5ccc069c403ebaf9f0171e9517f40e41";
            var mock = Substitute.For<IAccountService>();
            mock.Lookup("Mufasa", uri).Returns(new AuthenticationUserStub { Username = "Mufasa", Password = "Circle Of Life" });
            var realmRepos = Substitute.For<IRealmRepository>();
            realmRepos.GetRealm(Arg.Any<IHttpRequest>()).Returns("testrealm@host.com");
            var auth = new DigestAuthenticator(realmRepos, mock);
            var request = Substitute.For<IHttpRequest>();
            request.Headers["Authorization"].Returns(headerValue);
            request.Uri.Returns(uri);
            request.HttpMethod.Returns("GET");

            var user = auth.Authenticate(request);

            Assert.NotNull(user);
        }
    }
}
