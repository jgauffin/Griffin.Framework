using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Griffin.Net;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.MicroMsg.Serializers;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Channels
{
    public class TcpChannelTests : IDisposable
    {
        private ClientServerHelper _helper;

        public TcpChannelTests()
        {
            _helper = ClientServerHelper.Create();
        }

        [Fact]
        public void Assign_should_work_after_subscription()
        {
            var slice = new BufferSlice(new byte[65535], 0, 65535);
            var encoder = Substitute.For<IMessageEncoder>();
            var decoder = Substitute.For<IMessageDecoder>();
            object expected;

            var sut = new TcpChannel(slice, encoder, decoder);
            sut.MessageReceived += (channel, message) => expected = message;
            sut.Assign(_helper.Client);

        }

        [Fact]
        public void assign_without_subscribing_on_MessageReceived_means_that_messages_can_get_lost()
        {
            var slice = new BufferSlice(new byte[65535], 0, 65535);
            var encoder = Substitute.For<IMessageEncoder>();
            var decoder = Substitute.For<IMessageDecoder>();

            var sut = new TcpChannel(slice, encoder, decoder);
            Action actual = () => sut.Assign(_helper.Client);

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void should_listen_on_the_decoder_event()
        {
            var slice = new BufferSlice(new byte[65535], 0, 65535);
            var encoder = Substitute.For<IMessageEncoder>();
            var decoder = new FakeDecoder();
            object expected = null;

            var sut = new TcpChannel(slice, encoder, decoder);
            sut.MessageReceived += (channel, message) => expected = message;
            decoder.MessageReceived("Hello");

            expected.Should().Be("Hello");
        }


        [Fact]
        public void send_message()
        {
            var slice = new BufferSlice(new byte[65535], 0, 65535);
            var encoder = new FakeEncoder();
            var decoder = new FakeDecoder();
            object expected = null;

            var sut = new TcpChannel(slice, encoder, decoder);
            sut.MessageReceived += (channel, message) => expected = message;
            sut.Assign(_helper.Client);
            sut.Send("Hello world");

            
        }

        [Fact]
        public void Send_one_messages()
        {
            var slice1 = new BufferSlice(new byte[65535], 0, 65535);
            var encoder1 = new MicroMessageEncoder(new DataContractMessageSerializer());
            var decoder1 = new MicroMessageDecoder(new DataContractMessageSerializer());
            var slice2 = new BufferSlice(new byte[65535], 0, 65535);
            var encoder2 = new MicroMessageEncoder(new DataContractMessageSerializer());
            var decoder2 = new MicroMessageDecoder(new DataContractMessageSerializer());
            var evt = new ManualResetEvent(false);
            string actual = null;
            var sut2 = new TcpChannel(slice2, encoder2, decoder2);
            sut2.MessageReceived += (channel, message) =>
            {
                actual = message.ToString();
                evt.Set();
            };
            sut2.Assign(_helper.Server);

            var sut1 = new TcpChannel(slice1, encoder1, decoder1);
            sut1.MessageReceived += (channel, message) => { };
            sut1.Assign(_helper.Client);
            sut1.Send("Hello".PadRight(1000));

            evt.WaitOne(500).Should().BeTrue();
            actual.Should().StartWith("Hello");
        }


        [Fact]
        public void Send_500_messages()
        {
            var slice1 = new BufferSlice(new byte[65535], 0, 65535);
            var encoder1 = new MicroMessageEncoder(new DataContractMessageSerializer());
            var decoder1 = new MicroMessageDecoder(new DataContractMessageSerializer());
            var slice2 = new BufferSlice(new byte[65535], 0, 65535);
            var encoder2 = new MicroMessageEncoder(new DataContractMessageSerializer());
            var decoder2 = new MicroMessageDecoder(new DataContractMessageSerializer());
            var evt = new ManualResetEvent(false);
            var messages = new List<object>();
            var sut1 = new TcpChannel(slice1, encoder1, decoder1);
            sut1.MessageReceived += (channel, message) => { };
            sut1.Assign(_helper.Client);
            var sut2 = new TcpChannel(slice2, encoder2, decoder2);
            sut2.MessageReceived += (channel, message) =>
            {
                messages.Add(message);
                if (messages.Count == 500) evt.Set();
            };
            sut2.Assign(_helper.Server);

            for (int i = 0; i < 500; i++)
            {
                sut1.Send("Hello" + i + "".PadRight(1000));
            }

            sut1.Send("Hello world");

            evt.WaitOne(500).Should().BeTrue();
            for (int i = 0; i < 500; i++)
            {
                messages[i].ToString().Should().StartWith("Hello" + i);
            }
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
