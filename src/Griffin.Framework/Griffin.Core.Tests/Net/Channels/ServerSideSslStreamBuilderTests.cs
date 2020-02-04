using System;
using System.Diagnostics;
using System.Security.Authentication;
using System.Threading;
using FluentAssertions;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Xunit;

namespace Griffin.Core.Tests.Net.Channels
{
    public class ServerSideSslStreamBuilderTests
    {
        [Fact]
        public void should_Be_able_to_connect_if_protocol_and_common_name_is_matching()
        {
            var certificate = CertificateHelper.Create();
            var clientServer = ClientServerHelper.Create();
            var buffer = new StandAloneBuffer(new byte[65535], 0, 65535);
            var serverChannel = new TcpChannel();
            var clientChannel = new TcpChannel();


            var sut = new ServerSideSslStreamBuilder(certificate);
            sut.HandshakeTimeout = TimeSpan.FromMilliseconds(1000);
            ThreadPool.QueueUserWorkItem(x =>
            {
                var builder = new ClientSideSslStreamBuilder("GriffinNetworking");
                builder.Protocols = SslProtocols.Tls12;
                var chan = builder.Build(clientChannel, clientServer.Client);

            });
            var stream = sut.Build(serverChannel, clientServer.Server);


        }

        [Fact]
        public void should_fail_if_client_do_not_send_handshake_within_given_time_Frame()
        {
            var certificate = CertificateHelper.Create();
            var clientServer = ClientServerHelper.Create();
            var buffer = new StandAloneBuffer(new byte[65535], 0, 65535);
            var serverChannel = new TcpChannel();


            var sut = new ServerSideSslStreamBuilder(certificate);
            sut.HandshakeTimeout = TimeSpan.FromMilliseconds(100);
            Action actual = () => sut.Build(serverChannel, clientServer.Server);

            actual.Should().Throw<InvalidOperationException>();
        }


        [Fact]
        public void should_not_fail_before_given_time_frame()
        {
            var certificate = CertificateHelper.Create();
            var clientServer = ClientServerHelper.Create();
            var serverChannel = new TcpChannel();


            var sut = new ServerSideSslStreamBuilder(certificate);
            sut.HandshakeTimeout = TimeSpan.FromMilliseconds(500);
            var sw = Stopwatch.StartNew();
            Action actual = () => sut.Build(serverChannel, clientServer.Server);

            actual.Should().Throw<InvalidOperationException>();
            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(500);
        }


    }
}
