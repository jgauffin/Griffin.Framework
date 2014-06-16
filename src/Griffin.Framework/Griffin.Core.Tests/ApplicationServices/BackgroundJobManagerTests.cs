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
        public void anropar_ScopeCreated_eventet_när_jobben_ska_utföras()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            ScopeCreatedEventArgs actual = null;

            var sut = new BackgroundJobManager(sl);
            sut.ScopeCreated += (o, e) => actual = e;
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.Start();
            Thread.Sleep(100);


            actual.Should().NotBeNull();
        }

        [Fact]
        public void skapar_ett_IoC_scope_när_jobben_ska_utföras()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            sl.CreateScope().Returns(scope);

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.Start();
            Thread.Sleep(100);


            sl.Received().CreateScope();
        }

        [Fact]
        public void exekverar_funnet_jobb()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var job = Substitute.For<IBackgroundJob>();
            sl.CreateScope().Returns(scope);
            scope.ResolveAll<IBackgroundJob>().Returns<IEnumerable<IBackgroundJob>>(new[] {job});

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.Start();
            Thread.Sleep(100);

            job.Received().Execute();
        }

        [Fact]
        public void anropar_scope_closed_när_jobbet_är_klart()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            sl.CreateScope().Returns(scope);
            ScopeClosingEventArgs actual = null;

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.ScopeClosing += (o, e) => actual = e;
            sut.Start();
            Thread.Sleep(100);

            actual.Should().NotBeNull();
        }

        [Fact]
        public void anropar_scope_closing_även_om_ett_jobb_kastar_ett_undantag()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var job = Substitute.For<IBackgroundJob>();
            sl.CreateScope().Returns(scope);
            scope.ResolveAll<IBackgroundJob>().Returns(new[] { job });

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.Start();
            Thread.Sleep(100);

            job.Received().Execute();
        }

        [Fact]
        public void fortsätt_med_nästa_jobb_om_ett_falerar()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var job = Substitute.For<IBackgroundJob>();
            var job2 = Substitute.For<IBackgroundJob>();
            job.When(x => x.Execute()).Do(x => { throw new SqlNullValueException(); });
            sl.CreateScope().Returns(scope);
            scope.ResolveAll<IBackgroundJob>().Returns(new[] { job, job2 });

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.Start();
            Thread.Sleep(100);

            job2.Received().Execute();
        }

        [Fact]
        public void avbryt_om_CanContinue_sätts_till_false()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var job = Substitute.For<IBackgroundJob>();
            var job2 = Substitute.For<IBackgroundJob>();
            job.When(x => x.Execute()).Do(x => { throw new SqlNullValueException(); });
            sl.CreateScope().Returns(scope);
            scope.ResolveAll<IBackgroundJob>().Returns(new[] { job, job2 });

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.JobFailed += (o, e) => e.CanContinue = false;
            sut.Start();
            Thread.Sleep(100);

            job2.DidNotReceive().Execute();
        }

        // andra tråden skulle crasha om vi inte överlevde.
        [Fact]
        public void överlev_om_event_prenumreranterna_kastar_undantag()
        {
            var sl = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var job = Substitute.For<IBackgroundJob>();
            var job2 = Substitute.For<IBackgroundJob>();
            job.When(x => x.Execute()).Do(x => { throw new SqlNullValueException(); });
            sl.CreateScope().Returns(scope);
            scope.ResolveAll<IBackgroundJob>().Returns(new[] { job, job2 });

            var sut = new BackgroundJobManager(sl);
            sut.StartInterval = TimeSpan.FromSeconds(0);
            sut.JobFailed += (o, e) => { throw new Exception(); };
            sut.Start();
            Thread.Sleep(100);

            job2.DidNotReceive().Execute();
        }
    }
}
