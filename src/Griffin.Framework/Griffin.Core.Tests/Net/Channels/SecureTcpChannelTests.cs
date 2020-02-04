using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Xunit;

namespace Griffin.Core.Tests.Net.Channels
{
    public class SecureTcpChannelTests : IDisposable
    {
        private ClientServerHelper _helper;
        private X509Certificate2 _certificate;

        public SecureTcpChannelTests()
        {
            _helper = ClientServerHelper.Create();
            _certificate = new X509Certificate2(
                AppDomain.CurrentDomain.BaseDirectory + "\\Net\\cert\\GriffinNetworkingTemp.pfx", "mamma");
        }

        private SecureTcpChannel CreateClientChannel()
        {
            var streamBuilder = new ClientSideSslStreamBuilder("mamma");
            var channel = new SecureTcpChannel(streamBuilder);
            return channel;
        }

        private SecureTcpChannel CreateServerChannel()
        {
            var streamBuilder = new ServerSideSslStreamBuilder(_certificate);
            var channel = new SecureTcpChannel(streamBuilder);
            return channel;
        }

        [Fact]
        public async Task Should_be_able_to_Receive_bytes()
        {
            var buffer = new StandAloneBuffer(65535);
            var serverStream = new SslStream(new NetworkStream(_helper.Server));
            serverStream.BeginAuthenticateAsServer(_certificate, OnAuthenticated, serverStream);

            var sut = CreateClientChannel();
            sut.Assign(_helper.Client);
            serverStream.Write(new byte[] { 1, 2, 3, 4 });

            while (buffer.Count < 4)
            {
                var read = await sut.ReceiveAsync(buffer);
                buffer.Offset += read;
            }

            buffer.Count.Should().Be(4);
            buffer.Buffer[0].Should().Be(1);
            buffer.Buffer[3].Should().Be(4);
        }


        [Fact]
        public async Task Should_be_able_To_Send_messages()
        {
            var sendBuf = Encoding.UTF8.GetBytes("Hello world");
            var sut = CreateClientChannel();
            var stream = new SslStream(new NetworkStream(_helper.Server));
            stream.BeginAuthenticateAsServer(_certificate, OnAuthenticated, stream);
            sut.Assign(_helper.Client);
            await sut.SendAsync(sendBuf, 0, sendBuf.Length);

            //i do not know why this loop is required.
            //for some reason the send message is divided into two tcp packets 
            //when using SslStream.
            var bytesReceived = 0;
            var buf = new byte[65535];
            while (bytesReceived < sendBuf.Length)
                bytesReceived += stream.Read(buf, bytesReceived, 15);

            var actual = Encoding.ASCII.GetString(buf, 0, bytesReceived);
            actual.Should().Be("Hello world");
        }

        [Fact]
        public async Task send_close_message()
        {
            var sut = CreateClientChannel();
            var stream = new SslStream(new NetworkStream(_helper.Server));
            stream.BeginAuthenticateAsServer(_certificate, OnAuthenticated, stream);
            sut.Assign(_helper.Client);

            Assert.True(sut.IsConnected);

            await sut.CloseAsync();

            Assert.False(sut.IsConnected);
        }

        private void OnAuthenticated(IAsyncResult ar)
        {
            try
            {
                var stream = (SslStream)ar.AsyncState;
                stream.EndAuthenticateAsServer(ar);
            }
            catch { }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _helper.Dispose();
        }
    }
}
