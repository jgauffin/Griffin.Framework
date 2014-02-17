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
    public class ConnectHandlerTests
    {
        [Fact]
        public void version_is_required_according_to_the_specification()
        {
            var authService = Substitute.For<IAuthenticationService>();
            var frame = new BasicFrame("STOMP");
            var client = Substitute.For<IStompClient>();

            var sut = new ConnectHandler(authService, "Kickass");
            var actual = sut.Process(client, frame);

            actual.Should().NotBeNull();
            actual.Headers["version"].Should().Be("2.0");
            actual.Headers["message"].Should().Be("Missing the 'accept-version' header.");
        }

        [Fact]
        public void only_accepting_20_clients()
        {
            var authService = Substitute.For<IAuthenticationService>();
            var frame = new BasicFrame("STOMP");
            var client = Substitute.For<IStompClient>();
            frame.Headers["accept-version"] = "1.1";

            var sut = new ConnectHandler(authService, "Kickass");
            var actual = sut.Process(client, frame);

            actual.Should().NotBeNull();
            actual.Headers["version"].Should().Be("2.0");
            actual.Headers["message"].Should().Be("Only accepting stomp 2.0 clients.");
        }

        [Fact]
        public void accepting_20_when_multiple_versions_Are_supplied()
        {
            var authService = Substitute.For<IAuthenticationService>();
            var frame = new BasicFrame("STOMP");
            var client = Substitute.For<IStompClient>();
            frame.Headers["accept-version"] = "1.1,2.0";
            client.SessionKey.Returns(Guid.NewGuid().ToString());

            var sut = new ConnectHandler(authService, "Kickass");
            var actual = sut.Process(client, frame);

            actual.Should().NotBeNull();
            actual.Headers["version"].Should().Be("2.0");
            actual.Headers["message"].Should().BeNull();
            actual.Headers["session"].Should().NotBeNullOrEmpty();
            client.ReceivedWithAnyArgs().SetAsAuthenticated(null);
        }

        [Fact]
        public void always_accept_when_authentication_is_turned_off()
        {
            var authService = Substitute.For<IAuthenticationService>();
            var frame = new BasicFrame("STOMP");
            var client = Substitute.For<IStompClient>();
            frame.Headers["accept-version"] = "2.0";
            client.SessionKey.Returns(Guid.NewGuid().ToString());

            var sut = new ConnectHandler(authService, "Kickass");
            var actual = sut.Process(client, frame);

            actual.Should().NotBeNull();
            actual.Headers["version"].Should().Be("2.0");
            actual.Headers["server"].Should().Be("Kickass");
            actual.Headers["session"].Should().NotBeNullOrEmpty();
            client.ReceivedWithAnyArgs().SetAsAuthenticated(null);
        }

        [Fact]
        public void using_authentication_but_no_password_was_supplied()
        {
            var authService = Substitute.For<IAuthenticationService>();
            authService.IsActivated.Returns(true);
            var frame = new BasicFrame("STOMP");
            var client = Substitute.For<IStompClient>();
            frame.Headers["accept-version"] = "2.0";
            frame.Headers["login"] = "hello";
            client.SessionKey.Returns(Guid.NewGuid().ToString());

            var sut = new ConnectHandler(authService, "Kickass");
            var actual = sut.Process(client, frame);

            actual.Headers["message"].Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void using_authentication_but_no_username_was_supplied()
        {
            var authService = Substitute.For<IAuthenticationService>();
            authService.IsActivated.Returns(true);
            var frame = new BasicFrame("STOMP");
            var client = Substitute.For<IStompClient>();
            frame.Headers["accept-version"] = "2.0";
            frame.Headers["passcode"] = "world";
            client.SessionKey.Returns(Guid.NewGuid().ToString());

            var sut = new ConnectHandler(authService, "Kickass");
            var actual = sut.Process(client, frame);

            actual.Headers["message"].Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void using_authentication()
        {
            var authService = Substitute.For<IAuthenticationService>();
            authService.IsActivated.Returns(true);
            authService.Login("hello", "world").Returns(new LoginResponse() {IsSuccessful = true, Token = "mamma"});
            var frame = new BasicFrame("STOMP");
            var client = Substitute.For<IStompClient>();
            frame.Headers["accept-version"] = "2.0";
            frame.Headers["login"] = "hello";
            frame.Headers["passcode"] = "world";
            client.SessionKey.Returns(Guid.NewGuid().ToString());

            var sut = new ConnectHandler(authService, "Kickass");
            var actual = sut.Process(client, frame);

            actual.Should().NotBeNull();
            actual.Headers["version"].Should().Be("2.0");
            actual.Headers["server"].Should().Be("Kickass");
            actual.Headers["session"].Should().NotBeNull();
            client.ReceivedWithAnyArgs().SetAsAuthenticated("mamma");
        }

        [Fact]
        public void using_authentication_but_with_incorrect_password()
        {
            var authService = Substitute.For<IAuthenticationService>();
            authService.IsActivated.Returns(true);
            authService.Login("hello", "world").Returns(new LoginResponse() { IsSuccessful = false, Token = "mamma", Reason = "Incorrect password"});
            var frame = new BasicFrame("STOMP");
            var client = Substitute.For<IStompClient>();
            frame.Headers["accept-version"] = "2.0";
            frame.Headers["login"] = "hello";
            frame.Headers["passcode"] = "world";
            client.SessionKey.Returns(Guid.NewGuid().ToString());

            var sut = new ConnectHandler(authService, "Kickass");
            var actual = sut.Process(client, frame);

            actual.Headers["message"].Should().Be("Incorrect password");
        }
    }
}
