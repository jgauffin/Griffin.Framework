using System;
using System.Net;
using FluentAssertions;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Stomp.Broker;
using Griffin.Net.Protocols.Stomp.Frames;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp
{
    public class StompClientTests
    {
        [Fact]
        public void Abort_transaction_should_invoke_the_transactionManager()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();

            var sut = new StompClient(channel, transactionManager);
            sut.RollbackTransaction("abc");

            transactionManager.Received().Rollback("abc");
        }

        [Fact]
        public void Commit_transaction_should_invoke_the_transactionManager()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();

            var sut = new StompClient(channel, transactionManager);
            sut.CommitTransaction("abc");

            transactionManager.Received().Commit("abc");
        }


        [Fact]
        public void Enqueue_transaction_should_invoke_the_transactionManager()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();
            Action commitAction = () => { };
            Action rollbackAction = () => { };

            var sut = new StompClient(channel, transactionManager);
            sut.EnqueueInTransaction("abc", commitAction, rollbackAction);

            transactionManager.Received().Enqueue("abc", commitAction, rollbackAction);
        }


        [Fact]
        public void Begin_transaction_should_invoke_the_transactionManager()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();

            var sut = new StompClient(channel, transactionManager);
            sut.BeginTransaction("abc");

            transactionManager.Received().Begin("abc");
        }

        [Fact]
        public void set_sessionkey_during_authentication()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();

            var sut = new StompClient(channel, transactionManager);
            sut.SetAsAuthenticated("kdfjkd");
            var actual = sut.SessionKey;

            actual.Should().Be("kdfjkd");
        }

        [Fact]
        public void RemoteEndpoint_is_taken_from_The_channel()
        {
            var channel = Substitute.For<ITcpChannel>();
            channel.RemoteEndpoint.Returns(new IPEndPoint(IPAddress.Loopback, 5));
            var transactionManager = Substitute.For<ITransactionManager>();

            var sut = new StompClient(channel, transactionManager);
            var actual = sut.RemoteEndpoint;

            actual.Should().BeSameAs(channel.RemoteEndpoint);
        }

        [Fact]
        public void HasActiveTransactions_is_taken_from_the_TransactionManager()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();
            transactionManager.HasActiveTransactions.Returns(true);

            var sut = new StompClient(channel, transactionManager);
            var actual = sut.HasActiveTransactions;

            actual.Should().BeTrue();
        }

        [Fact]
        public void subscription_can_be_added_ok()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();
            var client = Substitute.For<IStompClient>();
            var subscription = new Subscription(client, "abc");

            var sut = new StompClient(channel, transactionManager);
            sut.AddSubscription(subscription);
            var actual = sut.SubscriptionExists("abc");

            actual.Should().BeTrue();
        }

        [Fact]
        public void cant_get_non_existent_subscription()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();

            var sut = new StompClient(channel, transactionManager);
            Action actual = () => sut.GetSubscription("abc");

            actual.ShouldThrow<NotFoundException>();
        }

        [Fact]
        public void get_existent_subscription()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();
            var client = Substitute.For<IStompClient>();
            var subscription = new Subscription(client, "abc");
            var frame = new BasicFrame("SEND");
            frame.AddHeader("message-id", "kdkd");
            subscription.AckType = "client-individual";
            subscription.Send(frame);

            var sut = new StompClient(channel, transactionManager);
            sut.AddSubscription(subscription);
            var actual = sut.GetSubscription("kdkd");

            actual.Should().BeSameAs(subscription);
        }

        [Fact]
        public void remove_subscription_should_really_remove_it()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();
            var client = Substitute.For<IStompClient>();
            var subscription = new Subscription(client, "abc");
            subscription.AckType = "client-individual";

            var sut = new StompClient(channel, transactionManager);
            sut.AddSubscription(subscription);
            var actual = sut.RemoveSubscription(subscription.Id);
            var actual2 = sut.RemoveSubscription(subscription.Id);

            actual.Should().BeSameAs(subscription);
            actual2.Should().BeNull();
        }

        [Fact]
        public void send_message_directly()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();
            var frame = new BasicFrame("SEND");

            var sut = new StompClient(channel, transactionManager);
            sut.Send(frame);

            channel.Received().Send(frame);
        }

        [Fact]
        public void send_must_include_a_frame()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();

            var sut = new StompClient(channel, transactionManager);
            Action actual = () => sut.Send(null);

            actual.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void cleanup_cleans_transactions_too()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();

            var sut = new StompClient(channel, transactionManager);
            sut.Cleanup();

            transactionManager.Received().Cleanup();
        }

        [Fact]
        public void is_for_Channel_checks_internal_channel_object()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();

            var sut = new StompClient(channel, transactionManager);
            var actual = sut.IsForChannel(channel);

            actual.Should().BeTrue();
        }

        [Fact]
        public void is_for_Channel_doesnt_work_for_unknown_Channel()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();

            var sut = new StompClient(channel, transactionManager);
            var actual = sut.IsForChannel(Substitute.For<ITcpChannel>());

            actual.Should().BeFalse();
        }

        [Fact]
        public void sent_message_should_be_pending_using_client_ack_type()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();
            var client = Substitute.For<IStompClient>();
            var subscription = new Subscription(client, "abc");
            var frame = new BasicFrame("MESSAGE");
            frame.AddHeader("message-id", "kdkd");
            subscription.AckType = "client";
            subscription.Send(frame);

            var sut = new StompClient(channel, transactionManager);
            sut.AddSubscription(subscription);
            var actual = sut.IsFramePending("kdkd");

            actual.Should().BeTrue();
        }



        [Fact]
        public void ack_messages_can_ack_all_since_we_dont_allow_multiple_pending_messages_with_client_individual_ack_type()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();
            var client = Substitute.For<IStompClient>();
            var subscription = new Subscription(client, "abc");
            subscription.AckType = "client";
            var frame1 = new BasicFrame("MESSAGE");
            frame1.AddHeader("message-id", "kdkd1");
            var frame2 = new BasicFrame("MESSAGE");
            frame2.AddHeader("message-id", "kdkd2");
            var frame3 = new BasicFrame("MESSAGE");
            frame3.AddHeader("message-id", "kdkd3");
            subscription.Send(frame1);
            subscription.Send(frame2);
            subscription.Send(frame3);

            var sut = new StompClient(channel, transactionManager);
            sut.AddSubscription(subscription);
            sut.AckMessages("kdkd2");
            var actual1 = sut.IsFramePending("kdkd1");
            var actual2 = sut.IsFramePending("kdkd2");
            var actual3 = sut.IsFramePending("kdkd3");

            actual1.Should().BeFalse();
            actual2.Should().BeFalse();
            actual3.Should().BeTrue();
        }
        
        [Fact]
        public void ack_unknown_message_is_an_error()
        {
            var channel = Substitute.For<ITcpChannel>();
            var transactionManager = Substitute.For<ITransactionManager>();
            var client = Substitute.For<IStompClient>();

            var sut = new StompClient(channel, transactionManager);
            Action actual = () => sut.AckMessages("ksksks");

            actual.ShouldThrow<NotFoundException>();
        }
    }
}
    