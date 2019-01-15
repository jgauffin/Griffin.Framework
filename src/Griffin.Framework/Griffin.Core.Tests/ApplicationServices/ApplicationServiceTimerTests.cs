using System;
using System.Threading;
using FluentAssertions;
using Griffin.ApplicationServices;
using Xunit;

namespace Griffin.Core.Tests.ApplicationServices
{
    public class ApplicationServiceTimerTests
    {
        [Fact]
        public void do_not_start_before_inital_delay()
        {

            var sut = new TestAppService();
            sut.Start();

            Thread.Sleep(300);
            sut.StartedEvent.WaitOne(0).Should().BeFalse();
        }

        [Fact]
        public void start_after_initial_delay()
        {

            var sut = new TestAppService();
            sut.Start();

            sut.StartedEvent.WaitOne(600).Should().BeTrue();
        }

        [Fact]
        public void should_not_be_able_to_Start_twice()
        {

            var sut = new TestAppService();
            sut.FirstInterval = TimeSpan.FromMilliseconds(0);
            sut.Start();
            sut.StartedEvent.WaitOne(100).Should().BeTrue();
            Action actual = sut.Start;

            actual.Should().Throw<InvalidOperationException>();
        }


        [Fact]
        public void started_service_should_be_marked_as_IsRunning_even_before_first_run()
        {

            var sut = new TestAppService();
            sut.FirstInterval = TimeSpan.Zero;
            sut.Start();

            sut.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void should_not_be_able_to_stop_a_not_running_service()
        {

            var sut = new TestAppService();
            Action actual = sut.Stop;

            actual.Should().Throw<InvalidOperationException>();
        }


        [Fact]
        public void should_be_able_to_restart()
        {

            var sut = new TestAppService();
            sut.FirstInterval = TimeSpan.FromMilliseconds(0);
            sut.Start();
            sut.StartedEvent.WaitOne(10000);
            sut.Stop();
            sut.StoppedEvent.WaitOne(10000);
            sut.StartedEvent.Reset();
            sut.Start();

            sut.StartedEvent.WaitOne(10000).Should().BeTrue();
        }


        [Fact]
        public void waits_on_handle_after_restarted()
        {



            var sut = new TestAppService();
            sut.FirstInterval = TimeSpan.FromMilliseconds(0);
            sut.Start();
            sut.StartedEvent.WaitOne(10000);
            sut.Stop();
            sut.StoppedEvent.WaitOne(10000);
            sut.StartedEvent.Reset();
            sut.Start();



            sut.StartedEvent.WaitOne(10000).Should().BeTrue();
        }

        [Fact]
        public void will_not_crash_when_Run_throws_exception_and_log_func_is_not_set()
        {
            var sut = new TestAppService();
            sut.WorkFunc = () => { throw new InvalidOperationException(); };

            sut.Start();
            sut.StartedEvent.WaitOne(10000);
            sut.Stop();
            var actual = sut.StoppedEvent.WaitOne(10000);

            actual.Should().BeFalse();
        }

        public class TestAppService : ApplicationServiceTimer
        {
            public ManualResetEvent StartedEvent = new ManualResetEvent(false);
            public ManualResetEvent StoppedEvent = new ManualResetEvent(false);
            public Action WorkFunc = () => { };


            protected override void Execute()
            {
                StartedEvent.Set();
                WorkFunc();
                StoppedEvent.Set();
            }
        }
    }
}
