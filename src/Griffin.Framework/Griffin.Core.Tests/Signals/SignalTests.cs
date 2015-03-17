using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Griffin.Signals;
using NCrunch.Framework;
using Xunit;

namespace Griffin.Core.Tests.Signals
{
    [ExclusivelyUses("SignalManager")]
    public class SignalTests
    {
        [Fact, ExclusivelyUses("SignalManager")]
        public void Do_not_expire_if_not_signaled()
        {
            var signalName = Guid.NewGuid().ToString();
            var actual = false;
            var sut = Signal.Create(signalName);
            sut.Raise("mofo");
            sut.Reset();
            sut.Expiration = TimeSpan.FromMilliseconds(1);
            sut.Suppressed += (sender, args) => actual = true;

            sut.Expire();

            actual.Should().BeFalse();
        }

        [Fact, ExclusivelyUses("SignalManager")]
        public void Expire_should_reset_signal_with_the_automated_indication()
        {
            var signalName = Guid.NewGuid().ToString();
            var actual = false;
            var sut = Signal.Create(signalName);
            sut.Raise("mofo");
            sut.Expiration = TimeSpan.FromMilliseconds(1);
            sut.Suppressed += (sender, args) => actual = args.Automated;

            sut.Expire();

            actual.Should().BeTrue();
        }

        [Fact]
        public void Raise_signal_on_first_invocation()
        {
            var sut = new Signal("name");

            var actual = sut.Raise("No reason");

            actual.Should().BeTrue();
        }

        [Fact]
        public void Raise_signal_on_first_invocation_should_trigger_event()
        {
            var sut = new Signal("name");
            var actual = false;

            sut.Raised += (sender, args) => actual = true;
            sut.Raise("No reason");

            actual.Should().BeTrue();
        }

        [Fact, ExclusivelyUses("SignalManager")]
        public void cant_create_a_signal_with_the_same_name()
        {
            var signalName = Guid.NewGuid().ToString();

            Signal.Create(signalName);
            Action actual = () => Signal.Create(signalName);

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void increase_signal_counter_on_every_Raise_invocation()
        {
            var sut = new Signal("myname");

            sut.Raise("hello");
            sut.Raise("hello");
            sut.Raise("hello");

            sut.RaiseCountSinceLastReset.Should().Be(3);
        }

        [Fact]
        public void raise_should_reset_idle_time()
        {
            var sut = new Signal("myname");

            sut.Raise("nothing");

            sut.IdleSinceUtc.Should().Be(DateTime.MinValue);
        }


        [Fact]
        public void raise_signal_on_second_invocation_should_not_raise_event()
        {
            var sut = new Signal("name");
            sut.Raise("no reason");
            var actual = false;

            sut.Raised += (sender, args) => actual = true;
            sut.Raise("No reason");

            actual.Should().BeFalse("because signal is already raised");
        }

        [Fact]
        public void raise_signal_on_second_invocation_should_not_raise_event_with_the_exception_overload()
        {
            var sut = new Signal("name");
            sut.Raise("no reason", new ExternalException());
            var actual = false;

            sut.Raised += (sender, args) => actual = true;
            sut.Raise("No reason");

            actual.Should().BeFalse("because signal is already raised");
        }

        [Fact]
        public void raise_signal_on_second_invocation_should_not_return_true()
        {
            var sut = new Signal("name");

            sut.Raise("no reason");
            var actual = sut.Raise("No reason");

            actual.Should().BeFalse("because signal is already raised");
        }

        [Fact]
        public void reset_should_invoke_event_when_signaled()
        {
            var sut = new Signal("name");
            sut.Raise("not here");
            var actual = false;

            sut.Suppressed += (sender, args) => actual = true;
            sut.Reset();

            actual.Should().BeTrue("because signal is not raised");
        }

        [Fact]
        public void reset_should_not_invoke_event_when_not_signaled()
        {
            var sut = new Signal("name");
            var actual = false;

            sut.Suppressed += (sender, args) => actual = true;
            sut.Reset();

            actual.Should().BeFalse("because signal is not raised");
        }

        [Fact]
        public void reset_should_not_return_false_when_not_being_signaled()
        {
            var sut = new Signal("name");

            var actual = sut.Reset();

            actual.Should().BeFalse("because signal is not raised");
        }

        [Fact]
        public void reset_should_not_return_true_when_being_signaled()
        {
            var sut = new Signal("name");
            sut.Raise("not here");

            var actual = sut.Reset();

            actual.Should().BeTrue("because signal is not raised");
        }

        [Fact]
        public void reset_signal_counter_on_reset()
        {
            var sut = new Signal("myname");
            sut.Raise("hello");
            sut.Raise("hello");
            sut.Raise("hello");

            sut.Reset();

            sut.RaiseCountSinceLastReset.Should().Be(0);
        }

        [Fact]
        public void reset_signal_should_set_idle_time()
        {
            var sut = new Signal("myname");
            sut.Raise("nothing");

            sut.Reset();

            sut.IdleSinceUtc.Should().BeCloseTo(DateTime.UtcNow);
        }

        [Fact]
        public void reset_signal_time_on_suppress()
        {
            var sut = new Signal("myname");
            sut.Raise("nothing");

            sut.Reset();

            sut.RaisedSinceUtc.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void set_signal_time_when_raising()
        {
            var sut = new Signal("myname");

            sut.Raise("nothing");

            sut.RaisedSinceUtc.Should().BeCloseTo(DateTime.UtcNow);
        }


        [Fact]
        public void signal_created_through_the_static_Create_should_get_raised_through_global_manager()
        {
            var signaled = false;
            var reseted = false;
            var signalName = Guid.NewGuid().ToString();
            Signal.SignalRaised += (sender, args) => signaled = true;
            Signal.SignalSuppressed += (sender, args) => reseted = true;

            var sut = Signal.Create(signalName);
            sut.Raise("this is the reason");
            sut.Reset();

            signaled.Should().BeTrue();
            reseted.Should().BeTrue();
        }

        [Fact, ExclusivelyUses("SignalManager")]
        public void signal_created_through_the_static_Raise_method_overload2_should_get_raised_through_global_manager()
        {
            var signaled = false;
            var reseted = false;
            var signalName = Guid.NewGuid().ToString();
            Signal.SignalRaised += (sender, args) => signaled = true;
            Signal.SignalSuppressed += (sender, args) => reseted = true;

            Signal.Raise(signalName, "myreason", new ExternalException());
            Signal.Reset(signalName);

            signaled.Should().BeTrue();
            reseted.Should().BeTrue();
        }

        [Fact, ExclusivelyUses("SignalManager")]
        public void signal_created_through_the_static_Raise_method_should_get_raised_through_global_manager()
        {
            var signaled = false;
            var reseted = false;
            var signalName = Guid.NewGuid().ToString();
            Signal.SignalRaised += (sender, args) => signaled = true;
            Signal.SignalSuppressed += (sender, args) => reseted = true;

            Signal.Raise(signalName, "myreason");
            Signal.Reset(signalName);

            signaled.Should().BeTrue();
            reseted.Should().BeTrue();
        }

        [Fact, ExclusivelyUses("SignalManager")]
        public void static_method_should_just_return_false_on_Reset_if_signal_do_not_exist()
        {
            var signalName = Guid.NewGuid().ToString();

            var actual = Signal.Reset(signalName);

            actual.Should().BeFalse();
        }
    }
}