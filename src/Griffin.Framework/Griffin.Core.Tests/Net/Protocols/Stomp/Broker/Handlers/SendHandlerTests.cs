using System;
using System.IO;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp.Broker;
using Griffin.Net.Protocols.Stomp.Broker.MessageHandlers;
using Griffin.Net.Protocols.Stomp.Broker.Services;
using Griffin.Net.Protocols.Stomp.Frames;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp.Broker.Handlers
{
    public class SendHandlerTests
    {
        [Fact]
        public void destination_is_required_according_to_the_specification()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("SEND");

            var handler = new SendHandler(repos);
            Action actual = () => handler.Process(client, msg);

            actual.ShouldThrow<BadRequestException>();
        }

        [Fact]
        public void content_type_is_required_if_a_body_is_present()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("SEND");
            msg.Headers["destination"] = "/queue/momas";
            msg.Headers["content-length"] = "10";
            msg.Body = new MemoryStream();

            var handler = new SendHandler(repos);
            Action actual = () => handler.Process(client, msg);

            actual.ShouldThrow<BadRequestException>();
        }

        [Fact]
        public void content_length_is_required_if_a_body_is_present()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("SEND");
            msg.Headers["destination"] = "/queue/momas";
            msg.Headers["content-type"] = "text/plain";
            msg.Body = new MemoryStream();

            var handler = new SendHandler(repos);
            Action actual = () => handler.Process(client, msg);

            actual.ShouldThrow<BadRequestException>();
        }

        [Fact]
        public void message_with_content_should_be_copied()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("SEND");
            msg.Headers["destination"] = "/queue/momas";
            msg.Headers["content-type"] = "text/plain";
            msg.Headers["content-length"] = "100";
            msg.Body = new MemoryStream(new byte[100], 0, 100);

            var handler = new SendHandler(repos);
            var actual = handler.Process(client, msg);

            actual.Body.Should().NotBeNull();
            actual.Body.Length.Should().Be(100);
            actual.Body.Position.Should().Be(0);
            actual.Headers["content-length"].Should().Be("100");
            actual.Headers["content-type"].Should().Be("text/plain");
        }

        [Fact]
        public void copy_all_unknown_headers()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("SEND");
            msg.Headers["destination"] = "/queue/momas";
            msg.Headers["feck"] = "fock";
            msg.Headers["fack"] = "fick";

            var handler = new SendHandler(repos);
            var actual = handler.Process(client, msg);

            actual.Headers["feck"].Should().Be("fock");
            actual.Headers["fack"].Should().Be("fick");
        }

        [Fact]
        public void enlist_transaction_messages()
        {
            var repos = Substitute.For<IQueueRepository>();
            var client = Substitute.For<IStompClient>();
            var msg = new BasicFrame("SEND");
            msg.Headers["destination"] = "/queue/momas";
            msg.Headers["transaction"] = "10";

            var handler = new SendHandler(repos);
            var actual = handler.Process(client, msg);

            actual.Should().BeNull();
            client.Received().EnqueueInTransaction("10", Arg.Any<Action>(), Arg.Any<Action>());
        }

    }
}
