using System;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp.Broker;
using Griffin.Net.Protocols.Stomp.Broker.MessageHandlers;
using Griffin.Net.Protocols.Stomp.Frames;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp.Broker.Handlers
{
    public class AbortHandlerTests
    {
        [Fact]
        public void cant_abort_without_transaction_identifier()
        {
            var frame = new BasicFrame("ABORT");
            var client = Substitute.For<IStompClient>();
            
            var sut = new AbortHandler();
            Action actual = () => sut.Process(client, frame);

            actual.ShouldThrow<BadRequestException>();
        }

        [Fact]
        public void abort_if_transaction_was_specified()
        {
            var frame = new BasicFrame("ABORT");
            frame.Headers["transaction"] = "aa";
            var client = Substitute.For<IStompClient>();

            var sut = new AbortHandler();
            sut.Process(client, frame);

            client.Received().RollbackTransaction("aa");
        }
    }
}
