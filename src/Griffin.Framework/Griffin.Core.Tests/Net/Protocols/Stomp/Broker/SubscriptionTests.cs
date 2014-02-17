using System;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp.Broker;
using Griffin.Net.Protocols.Stomp.Frames;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp.Broker
{
    public class SubscriptionTests
    {
        [Fact]
        public void init_class_properly()
        {
            var client = Substitute.For<IStompClient>();

            var sut = new Subscription(client, "dkkdkd");

            sut.Id.Should().Be("dkkdkd");
        }


        [Fact]
        public void may_not_send_while_waiting_on_individual_ack()
        {
            var client = Substitute.For<IStompClient>();

            var sut = new Subscription(client, "dkkdkd");
            sut.AckType = "client-individual";
            sut.Send(new BasicFrame("MESSAGE"));
            Action actual = () => sut.Send(new BasicFrame("MESSAGE"));

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void may_not_send_when_having_too_many_pending_messages()
        {
            var client = Substitute.For<IStompClient>();

            var sut = new Subscription(client, "dkkdkd");
            sut.MaxMessagesPerSecond = 30;
            sut.AckType = "client";
            for (int i = 0; i < 20; i++)
                sut.Send(new BasicFrame("MESSAGE"));

            Action actual = () => sut.Send(new BasicFrame("MESSAGE"));

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void throttle_messages()
        {
            var client = Substitute.For<IStompClient>();

            var sut = new Subscription(client, "dkkdkd");
            sut.MaxMessagesPerSecond = 2;
            sut.AckType = "client";
            sut.Send(new BasicFrame("MESSAGE"));
            sut.Send(new BasicFrame("MESSAGE"));
            Action actual = () => sut.Send(new BasicFrame("MESSAGE"));

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void throttle_auto_too()
        {
            var client = Substitute.For<IStompClient>();

            var sut = new Subscription(client, "dkkdkd");
            sut.MaxMessagesPerSecond = 2;
            sut.AckType = "auto";
            sut.Send(new BasicFrame("MESSAGE"));
            sut.Send(new BasicFrame("MESSAGE"));
            Action actual = () => sut.Send(new BasicFrame("MESSAGE"));

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void get_existent_subscription()
        {
            var client = Substitute.For<IStompClient>();
            var subscription = new Subscription(client, "abc");
            var frame = new BasicFrame("MESSAGE");
            frame.AddHeader("message-id", "kdkd");
            subscription.AckType = "client-individual";
            subscription.Send(frame);

            var actual = subscription.IsMessagePending("kdkd");

            actual.Should().BeTrue();
        }

        [Fact]
        public void just_ack_messages_up_to_the_given_one()
        {
            var client = Substitute.For<IStompClient>();
            var frame1 = new BasicFrame("MESSAGE");
            var frame2 = new BasicFrame("MESSAGE");
            var frame3 = new BasicFrame("MESSAGE");
            frame1.AddHeader("message-id", "kdkd1");
            frame2.AddHeader("message-id", "kdkd2");
            frame3.AddHeader("message-id", "kdkd3");

            var sut = new Subscription(client, "abc");
            sut.AckType = "client";
            sut.Send(frame1);
            sut.Send(frame2);
            sut.Send(frame3);
            sut.Ack("kdkd2");

            sut.IsMessagePending("kdkd1").Should().BeFalse();
            sut.IsMessagePending("kdkd2").Should().BeFalse();
            sut.IsMessagePending("kdkd3").Should().BeTrue();
        }
    }
}
