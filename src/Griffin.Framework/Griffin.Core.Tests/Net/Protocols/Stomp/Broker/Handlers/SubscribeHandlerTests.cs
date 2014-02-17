using System;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp.Broker;
using Griffin.Net.Protocols.Stomp.Broker.MessageHandlers;
using Griffin.Net.Protocols.Stomp.Broker.Services;
using Griffin.Net.Protocols.Stomp.Frames;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp.Broker.Handlers
{
    public class SubscribeHandlerTests
    {
        [Fact]
        public void subscription_id_must_be_specified_according_to_the_specification()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("SUBSCRIBE");

            var sut = new SubscribeHandler(repos);
            Action actual = () => sut.Process(client, msg);

            actual.ShouldThrow<BadRequestException>();
        }

        [Fact]
        public void may_not_subscribe_on_previously_created_subscription()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("SUBSCRIBE");
            msg.Headers["id"] = "123";
            client.SubscriptionExists("123").Returns(true);

            var sut = new SubscribeHandler(repos);
            Action actual = () => sut.Process(client, msg);

            actual.ShouldThrow<BadRequestException>();
        }

       
        [Fact]
        public void successful_subcribe()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("SUBSCRIBE");
            msg.Headers["id"] = "123";
            msg.Headers["destination"] = "/queue/mamma";
            repos.Get("/queue/mamma").Returns(new StompQueue());

            var sut = new SubscribeHandler(repos);
            var actual = sut.Process(client, msg);

        
        }

    }
}
