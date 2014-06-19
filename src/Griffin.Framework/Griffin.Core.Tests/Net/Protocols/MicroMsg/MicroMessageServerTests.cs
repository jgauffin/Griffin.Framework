using Griffin.Net;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.MicroMsg;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.MicroMsg
{
    public class MicroMessageServerTests
    {
        [Fact]
        public void Test()
        {
            var sut = new MicroMessageTcpListener();
        }
    }
}
