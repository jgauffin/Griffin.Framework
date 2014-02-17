using System;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp.Broker;
using Griffin.Net.Protocols.Stomp.Broker.MessageHandlers;
using Griffin.Net.Protocols.Stomp.Frames;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp.Broker.Handlers
{
    public class AckHandlerTests
    {
        [Fact]
        public void cant_ack_without_message_id()
        {
            var frame = new BasicFrame("ACK");
            var client = Substitute.For<IStompClient>();

            var sut = new AckHandler();
            Action actual = () => sut.Process(client, frame);

            actual.ShouldThrow<BadRequestException>();
        }

        [Fact]
        public void cant_ack_if_message_Was_not_found()
        {
            var frame = new BasicFrame("ACK");
            frame.Headers["id"] = "aa";
            var client = Substitute.For<IStompClient>();
            client.IsFramePending("aa").Returns(false);

            var sut = new AckHandler();
            Action actual = () => sut.Process(client, frame);

            actual.ShouldThrow<BadRequestException>();
        }

        [Fact]
        public void enqueue_if_transaction_was_specified()
        {
            var frame = new BasicFrame("ACK");
            frame.Headers["id"] = "aa";
            frame.Headers["transaction"] = "sdfsd";
            var client = Substitute.For<IStompClient>();
            var subscription = Substitute.For<Subscription>(client, "aa");
            client.IsFramePending("aa").Returns(true);
            client.GetSubscription("aa").Returns(subscription);

            var sut = new AckHandler();
            sut.Process(client, frame);

            client.Received().EnqueueInTransaction("sdfsd", Arg.Any<Action>(), Arg.Any<Action>());
            subscription.DidNotReceive().Ack("aa");
        }

        [Fact]
        public void ack()
        {
            var frame = new BasicFrame("ACK");
            frame.Headers["id"] = "aa";
            var client = Substitute.For<IStompClient>();
            var subscription = Substitute.For<Subscription>(client, "aa");
            client.IsFramePending("aa").Returns(true);
            client.GetSubscription("aa").Returns(subscription);

            var sut = new AckHandler();
            sut.Process(client, frame);

            subscription.Received().Ack("aa");
        }
    }
}
