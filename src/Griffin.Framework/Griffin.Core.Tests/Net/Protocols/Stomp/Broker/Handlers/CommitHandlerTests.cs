using System;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp.Broker;
using Griffin.Net.Protocols.Stomp.Broker.MessageHandlers;
using Griffin.Net.Protocols.Stomp.Frames;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp.Broker.Handlers
{
    public class CommitHandlerTests
    {
        [Fact]
        public void transaction_must_be_specified()
        {
            var frame = new BasicFrame("COMMIT");
            var client = Substitute.For<IStompClient>();

            var sut = new CommitHandler();
            Action actual = () => sut.Process(client, frame);

            actual.ShouldThrow<BadRequestException>();
        }

        [Fact]
        public void succeed_if_transaction_was_specified()
        {
            var frame = new BasicFrame("COMMIT");
            frame.Headers["transaction"] = "aa";
            var client = Substitute.For<IStompClient>();

            var sut = new CommitHandler();
            sut.Process(client, frame);

            client.Received().CommitTransaction("aa");
        }
    }
}