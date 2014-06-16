using System;
using System.Threading;
using FluentAssertions;
using Griffin.ApplicationServices;
using Xunit;

namespace Griffin.Core.Tests.ApplicationServices
{
    
    public class ApplicationServiceThreadTests
    {
        [Fact]
        public void start_service_ok()
        {

            var sut = new TestAppService();
            sut.Start();

            sut.StartedEvent.WaitOne(100).Should().BeTrue();
        }

        [Fact]
        public void should_not_be_able_to_Start_twice()
        {

            var sut = new TestAppService();
            sut.Start();
            sut.StartedEvent.WaitOne(100).Should().BeTrue();
            Action actual = sut.Start;

            actual.ShouldThrow<InvalidOperationException>();
        }


        [Fact]
        public void started_service_should_be_marked_as_IsRunning()
        {

            var sut = new TestAppService();
            sut.Start();
            sut.StartedEvent.WaitOne(100).Should().BeTrue();
            Action actual = sut.Start;

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void should_not_be_able_to_stop_a_not_running_service()
        {

            var sut = new TestAppService();
            Action actual = sut.Stop;

            actual.ShouldThrow<InvalidOperationException>();
        }


        [Fact]
        public void should_be_able_to_restart()
        {

            var sut = new TestAppService();
            sut.Start();
            sut.StartedEvent.WaitOne(100);
            sut.Stop();
            sut.StoppedEvent.WaitOne(100);
            sut.StartedEvent.Reset();
            sut.Start();

            sut.StartedEvent.WaitOne(100).Should().BeTrue();
        }


        [Fact]
        public void waits_on_handle_after_restarted()
        {



            var sut = new TestAppService();
            sut.WorkFunc = handle => handle.WaitOne(500);
            sut.Start();
            sut.StartedEvent.WaitOne(100);
            sut.Stop();
            sut.StoppedEvent.WaitOne(100);
            sut.StartedEvent.Reset();
            sut.Start();



            sut.StartedEvent.WaitOne(100).Should().BeTrue();
        }

        [Fact]
        public void will_not_crash_when_Run_throws_exception_and_log_func_is_not_set()
        {
            var sut = new TestAppService();
            sut.WorkFunc = handle => { throw new InvalidOperationException(); };

            sut.Start();
            sut.StartedEvent.WaitOne(100);
            sut.Stop();
            var actual = sut.StoppedEvent.WaitOne(10000);

            actual.Should().BeFalse();
        }

        public class TestAppService : ApplicationServiceThread
        {
            public ManualResetEvent StartedEvent = new ManualResetEvent(false);
            public ManualResetEvent StoppedEvent = new ManualResetEvent(false);
            public Action<WaitHandle> WorkFunc = handle => { };

            /// <summary>
            ///     Run your logic.
            /// </summary>
            /// <param name="shutdownHandle">Being triggered when your method should stop running.</param>
            /// <example>
            ///     <code>
            /// protected void Run(WaitHandle shutdownHandle)
            /// {
            ///     while (true)
            ///     {
            ///         try
            ///         {
            ///             // pause 100ms between each loop iteration.
            ///             // you can specify 0 too
            ///             if (shutdownHandle.Wait(100))
            ///                 break;
            /// 
            ///             // do actual logic here.
            ///         } 
            ///         catch (Exception ex)
            ///         {
            ///             // shutdown thread if it's a DB exception
            ///             // thread will be started again by the ApplicationServiceManager
            ///             if (Exception is DataException)
            ///                 throw;
            /// 
            ///             _log.Error("Opps", ex);
            ///         }
            ///     }
            /// }
            /// </code>
            /// </example>
            protected override void Run(WaitHandle shutdownHandle)
            {
                StartedEvent.Set();
                WorkFunc(shutdownHandle);
                shutdownHandle.WaitOne();
                StoppedEvent.Set();
            }
        }
    }
}
