using System;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp.Broker;
using Griffin.Net.Protocols.Stomp.Broker.MessageHandlers;
using Griffin.Net.Protocols.Stomp.Frames;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp.Broker.Handlers
{
    public class BeginHandlerTests
    {
        [Fact]
        public void transaction_must_be_specified()
        {
            var frame = new BasicFrame("BEGIN");
            var client = Substitute.For<IStompClient>();

            var sut = new BeginHandler();
            Action actual = () => sut.Process(client, frame);

            actual.ShouldThrow<BadRequestException>();
        }


        [Fact]
        public void may_not_begin_an_already_created_transaction()
        {
            var frame = new BasicFrame("BEGIN");
            frame.Headers["transaction"] = "aa";
            var client = Substitute.For<IStompClient>();
            client.When(x => x.BeginTransaction("aa"))
                .Do(x => { throw new InvalidOperationException("Transaction already exist"); });

            var sut = new BeginHandler();
            Action actual = () => sut.Process(client, frame);

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void abort_if_transaction_was_specified()
        {
            var frame = new BasicFrame("BEGIN");
            frame.Headers["transaction"] = "aa";
            var client = Substitute.For<IStompClient>();

            var sut = new BeginHandler();
            sut.Process(client, frame);

            client.Received().BeginTransaction("aa");
        }
    }
}