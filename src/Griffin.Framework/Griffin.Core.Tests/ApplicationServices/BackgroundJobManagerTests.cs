using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Threading;
using FluentAssertions;
using Griffin.ApplicationServices;
using Griffin.Container;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.ApplicationServices
{
    
    public class BackgroundJobManagerTests
    {
        [Fact]
        public void trigger_ScopeCreated_event_before_running_the_job()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            ScopeCreatedEventArgs actual = null;
            var job = Substitute.For<IBackgroundJob>();
            sl.CreateScope().Returns(scope);
            scope.Resolve(job.GetType()).Returns(job);
            scope.ResolveAll<IBackgroundJob>().Returns(new[] { job });

            var sut = new BackgroundJobManager(sl);
            sut.ScopeCreated += (o, e) => actual = e;
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.Start();
            Thread.Sleep(100);


            actual.Should().NotBeNull();
        }

        [Fact]
        public void create_a_IoC_scope_before_running_a_job()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            sl.CreateScope().Returns(scope);
            var job = Substitute.For<IBackgroundJob>();
            scope.Resolve(job.GetType()).Returns(job);
            scope.ResolveAll<IBackgroundJob>().Returns(new[] { job });

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.Start();
            Thread.Sleep(100);


            sl.Received(2).CreateScope();
        }

        [Fact]
        public void execute_found_job()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var job = Substitute.For<IBackgroundJob>();
            sl.CreateScope().Returns(scope);
            scope.Resolve(job.GetType()).Returns(job);
            scope.ResolveAll<IBackgroundJob>().Returns(new[] {job});

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.Start();
            Thread.Sleep(100);

            job.Received().Execute();
        }

        [Fact]
        public void trigger_ScopeClosed_before_closing_it_after_job_Execution()
        {
            var sl = Substitute.For<IContainer>();
            var job = Substitute.For<IBackgroundJob>();
            var scope = Substitute.For<IContainerScope>();
            sl.CreateScope().Returns(scope);
            scope.Resolve(job.GetType()).Returns(job);
            scope.ResolveAll<IBackgroundJob>().Returns(new[] { job });
            ScopeClosingEventArgs actual = null;

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.ScopeClosing += (o, e) => actual = e;
            sut.Start();
            Thread.Sleep(100);

            actual.Should().NotBeNull();
        }

        [Fact]
        public void trigger_ScopeClosed_before_closing_it_even_if_job_execution_fails()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var job = Substitute.For<IBackgroundJob>();
            sl.CreateScope().Returns(scope);
            scope.Resolve(job.GetType()).Returns(job);
            scope.ResolveAll<IBackgroundJob>().Returns(new[] { job });

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.Start();
            Thread.Sleep(100);

            job.Received().Execute();
        }

        [Fact]
        public void run_next_job_even_if_first_fails()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var job = Substitute.For<IBackgroundJob>();
            var job2 = Substitute.For<IBackgroundJob>();
            job.When(x => x.Execute()).Do(x => { throw new SqlNullValueException(); });
            sl.CreateScope().Returns(scope);
            scope.Resolve(job.GetType()).Returns(job);
            scope.Resolve(job2.GetType()).Returns(job2);
            scope.ResolveAll<IBackgroundJob>().Returns(new[] { job, job2 });

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.Start();
            Thread.Sleep(100);

            job2.Received().Execute();
        }

      
        [Fact]
        public void consume_exceptions_thrown_by_event_subscribers()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var job = Substitute.For<IBackgroundJob>();
            var job2 = Substitute.For<IBackgroundJob>();
            job.When(x => x.Execute()).Do(x => { throw new SqlNullValueException(); });
            sl.CreateScope().Returns(scope);
            scope.ResolveAll<IBackgroundJob>().Returns(new[] { job, job2 });
            scope.Resolve(job.GetType()).Returns(job);
            scope.Resolve(job2.GetType()).Returns(job2);

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.JobFailed += (o, e) => { throw new Exception(); };
            sut.Start();
            Thread.Sleep(100);

            job2.Received().Execute();
        }
    }
}
