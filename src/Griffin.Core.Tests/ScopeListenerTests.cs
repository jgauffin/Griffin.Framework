using System;
using NSubstitute;
using Xunit;

namespace Griffin.Framework.Tests
{
    public class ScopeListenerTests
    {
        public ScopeListenerTests()
        {
            ScopeListeners.Clear();
        }

        [Fact]
        public void AddMany()
        {
            var listener1 = Substitute.For<IScopeListener>();
            var listener2 = Substitute.For<IScopeListener>();
            var listener3 = Substitute.For<IScopeListener>();
            ScopeListeners.Subscribe(listener1);
            ScopeListeners.Subscribe(listener2);
            ScopeListeners.Subscribe(listener3);
            var guid = Guid.NewGuid();

            var publisher = ScopeListeners.Register("hello");
            publisher.TriggerStarted(guid);

            listener1.Received(1).ScopeStarted(guid);
            listener2.Received(1).ScopeStarted(guid);
            listener3.Received(1).ScopeStarted(guid);
        }

        [Fact]
        public void Starting()
        {
            var listener = Substitute.For<IScopeListener>();
            ScopeListeners.Subscribe(listener);
            var guid = Guid.NewGuid();

            var publisher = ScopeListeners.Register("hello");
            publisher.TriggerStarting(guid);

            listener.Received().ScopeStarting(guid);
        }

        [Fact]
        public void Started()
        {
            var listener = Substitute.For<IScopeListener>();
            ScopeListeners.Subscribe(listener);
            var guid = Guid.NewGuid();

            var publisher = ScopeListeners.Register("hello");
            publisher.TriggerStarted(guid);

            listener.Received().ScopeStarted(guid);
        }

        [Fact]
        public void Ending()
        {
            var listener = Substitute.For<IScopeListener>();
            ScopeListeners.Subscribe(listener);
            var guid = Guid.NewGuid();

            var publisher = ScopeListeners.Register("hello");
            publisher.TriggerEnding(guid);

            listener.Received().ScopeEnding(guid);
        }

        [Fact]
        public void Ended()
        {
            var listener = Substitute.For<IScopeListener>();
            ScopeListeners.Subscribe(listener);
            var guid = Guid.NewGuid();

            var publisher = ScopeListeners.Register("hello");
            publisher.TriggerEnded(guid);

            listener.Received().ScopeEnded(guid);
        }

    }
}
