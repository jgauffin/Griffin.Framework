using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Net;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.Strings;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Channels
{
    public class MessagingClientTests : IDisposable
    {
        private readonly ClientServerHelper _helper;
        private readonly X509Certificate2 _certificate;

        public MessagingClientTests()
        {
            _helper = ClientServerHelper.Create();
            _certificate = new X509Certificate2(
                AppDomain.CurrentDomain.BaseDirectory + "\\Net\\cert\\GriffinNetworkingTemp.pfx", "mamma");
        }


        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _certificate.Dispose();
            _helper.Dispose();
        }

        private MessagingClient CreateClientChannel(IBufferSegment segment, IMessageEncoder encoder,
            IMessageDecoder decoder)
        {
            var streamBuilder = new ClientSideSslStreamBuilder("mamma");
            var channel = new MessagingClient(encoder, decoder, segment) {Certificate = streamBuilder};
            return channel;
        }

        private SecureTcpChannel CreateServerChannel(IBufferSegment segment, IMessageEncoder encoder,
            IMessageDecoder decoder)
        {
            var streamBuilder = new ServerSideSslStreamBuilder(_certificate);
            var channel = new SecureTcpChannel(streamBuilder);
            return channel;
        }

        private void OnAuthenticated(IAsyncResult ar)
        {
            try
            {
                var stream = (SslStream) ar.AsyncState;
                stream.EndAuthenticateAsServer(ar);
            }
            catch
            {
            }
        }

        [Fact]
        public async Task Receive_one_message()
        {
            var readBuffer = new StandAloneBuffer(65535);
            var encoder1 = new StringEncoder();
            var decoder1 = new StringDecoder();
            var expected = "Hello".PadRight(5000);
            var outBuffer = new byte[expected.Length + 4];
            var serverStream = new SslStream(new NetworkStream(_helper.Server));
            BitConverter2.GetBytes(expected.Length, outBuffer, 0);
            Encoding.UTF8.GetBytes(expected, 0, expected.Length, outBuffer, 4);
            serverStream.BeginAuthenticateAsServer(_certificate, OnAuthenticated, serverStream);

            var sut1 = CreateClientChannel(readBuffer, encoder1, decoder1);
            sut1.Assign(_helper.Client);
            serverStream.Write(outBuffer);
            var actual = await sut1.ReceiveAsync();

            actual.Should().Be(expected);
        }

        [Fact]
        public async Task send_close_message()
        {
            var receiveBuffer = new StandAloneBuffer(65535);
            var encoder = new StringEncoder();
            var decoder = new StringDecoder();
            var serverStream = new SslStream(new NetworkStream(_helper.Server));
            serverStream.BeginAuthenticateAsServer(_certificate, OnAuthenticated, serverStream);

            var sut = CreateClientChannel(receiveBuffer, encoder, decoder);
            sut.Assign(_helper.Client);

            Assert.True(sut.IsConnected);

            await sut.CloseAsync();

            Assert.False(sut.IsConnected);
        }


        [Fact]
        public async Task send_message()
        {
            var receiveBuffer = new StandAloneBuffer(65535);
            var encoder = new StringEncoder();
            var decoder = new StringDecoder();
            var serverStream = new SslStream(new NetworkStream(_helper.Server));
            serverStream.BeginAuthenticateAsServer(_certificate, OnAuthenticated, serverStream);

            var sut = CreateClientChannel(receiveBuffer, encoder, decoder);
            sut.Assign(_helper.Client);
            await sut.SendAsync("Hello world");

            //i do not know why this loop is required.
            //for some reason the send message is divided into two tcp packets 
            //when using SslStream.
            var bytesReceived = 0;
            var buf = new byte[65535];
            while (bytesReceived < 15)
            {
                var bytesRead = serverStream.Read(buf, bytesReceived, 15);
                if (bytesRead == 0)
                    throw new InvalidOperationException("Failed!");
                bytesReceived += bytesRead;
            }

            var actual = Encoding.ASCII.GetString(buf, 4, bytesReceived - 4); // string encoder have a length header.
            actual.Should().Be("Hello world");
        }
    }
}