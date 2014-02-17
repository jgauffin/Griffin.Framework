using System;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp;
using Griffin.Net.Protocols.Stomp.Broker;
using Griffin.Net.Protocols.Stomp.Broker.MessageHandlers;
using Griffin.Net.Protocols.Stomp.Broker.Services;
using Griffin.Net.Protocols.Stomp.Frames;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp.Broker.Handlers
{
    public class NackHandlerTests
    {
        [Fact]
        public void cant_nack_without_message_id()
        {
            var frame = new BasicFrame("NACK");
            var client = Substitute.For<IStompClient>();
            var repos = Substitute.For<IQueueRepository>();

            var sut = new NackHandler(repos);
            Action actual = () => sut.Process(client, frame);

            actual.ShouldThrow<BadRequestException>();
        }

        [Fact]
        public void cant_nack_if_message_Was_not_found()
        {
            var frame = new BasicFrame("NACK");
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            frame.Headers["id"] = "aa";
            client.IsFramePending("aa").Returns(false);

            var sut = new NackHandler(repos);
            Action actual = () => sut.Process(client, frame);

            actual.ShouldThrow<BadRequestException>();
        }

        [Fact]
        public void enqueue_if_transaction_was_specified()
        {
            var frame = new BasicFrame("NACK");
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var subscription = Substitute.For<Subscription>(client, "aa");
            var queue = Substitute.For<IStompQueue>();
            subscription.QueueName = "Q";
            repos.Get("Q").Returns(queue);
            frame.Headers["id"] = "aa";
            frame.Headers["transaction"] = "sdfsd";
            client.IsFramePending("aa").Returns(true);
            client.GetSubscription("aa").Returns(subscription);

            var sut = new NackHandler(repos);
            sut.Process(client, frame);

            client.Received().EnqueueInTransaction("sdfsd", Arg.Any<Action>(), Arg.Any<Action>());
            subscription.DidNotReceive().Nack("aa");
            queue.DidNotReceiveWithAnyArgs().Enqueue(null);
        }

        
        [Fact]
        public void nack()
        {
            
            var frame = new BasicFrame("NACK");
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var subscription = Substitute.For<Subscription>(client, "aa");
            var queue = Substitute.For<IStompQueue>();
            subscription.Nack("aa").Returns(new IFrame[]{frame});
            subscription.QueueName = "Q";
            repos.Get("Q").Returns(queue);
            frame.Headers["id"] = "aa";
            client.IsFramePending("aa").Returns(true);
            client.GetSubscription("aa").Returns(subscription);

            var sut = new NackHandler(repos);
            sut.Process(client, frame);

            subscription.Received().Nack("aa");
            queue.Received().Enqueue(frame);
        }
    }
}
