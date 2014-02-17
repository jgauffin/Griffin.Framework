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
    public class UnsubscribeHandlerTests
    {
        [Fact]
        public void id_is_required()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("UNSUBSCRIBE");

            var sut = new UnsubscribeHandler(repos);
            Action actual =()=> sut.Process(client, msg);

            actual.ShouldThrow<BadRequestException>();
        }

        [Fact]
        public void subscription_must_exist()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("UNSUBSCRIBE");
            msg.Headers["id"] = "1";

            var sut = new UnsubscribeHandler(repos);
            Action actual = () => sut.Process(client, msg);

            actual.ShouldThrow<BadRequestException>();
        }


        [Fact]
        public void Test()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("UNSUBSCRIBE");
            msg.Headers["id"] = "1";
            client.RemoveSubscription("1").Returns(new Subscription(client, "1"));

            var sut = new UnsubscribeHandler(repos);
            var actual = sut.Process(client, msg);

            actual.Should().BeNull();
        }
    }
}
