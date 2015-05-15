using Griffin.Net;
using Griffin.Net.Buffers;

namespace DemoTest
{
    public class MyProtocolClient : ChannelTcpClient
    {
        public MyProtocolClient() : base(new MyProtocolEncoder(), new MyProtocolDecoder(), new BufferSlice(new byte[65535], 0, 65535))
        {
        }

    }
}