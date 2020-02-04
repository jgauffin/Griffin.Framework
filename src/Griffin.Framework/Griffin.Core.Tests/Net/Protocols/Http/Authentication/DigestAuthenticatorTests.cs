using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Authentication;
using Griffin.Net.Protocols.Http.Middleware.Authentication;
using Griffin.Net.Protocols.Http.Middleware.Authentication.Digest;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Http.Authentication
{
    public class DigestAuthenticatorTests
    {
        ClaimsPrincipal _principal = new ClaimsPrincipal(new ClaimsIdentity(new []
        {
            new Claim(ClaimTypes.NameIdentifier, "Mufasa"),
            new Claim(ClaimTypes.Thumbprint, "Circle Of Life"), 
        }));

        /// <summary>
        /// Values are taken from the Wikipedia Article
        /// </summary>
        [Fact]
        public void Test()
        {
            var uri = new Uri("http://testrealm@host.com/dir/index.html");
            var headerValue =
                @"Digest username=""Mufasa"", realm=""testrealm@host.com"", nonce=""dcd98b7102dd2f0e8b11d0f600bfb0c093"", uri=""/dir/index.html"", qop=auth, nc=00000001, cnonce=""0a4f113b"", response=""6629fae49393a05397450978507c4ef1"", opaque=""5ccc069c403ebaf9f0171e9517f40e41";
            var accountService = Substitute.For<IAccountService>();
            accountService.LoadAsync(uri, "Mufasa")
            .Returns(_principal);
            var realmRepos = Substitute.For<IRealmRepository>();
            realmRepos.GetRealm(Arg.Any<HttpRequest>()).Returns("testrealm@host.com");
            var auth = new DigestAuthenticator(realmRepos, accountService);
            var request = Substitute.For<HttpRequest>();
            request.Headers["Authorization"].Returns(headerValue);
            request.Uri.Returns(uri);
            request.HttpMethod.Returns("GET");
            var context = new HttpContext(new ChannelData());
            context.Request = request;

            var user = auth.Process(context, () => Task.CompletedTask);

            Assert.NotNull(user);
        }
    }
}
