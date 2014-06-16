using System;
using System.Threading;
using FluentAssertions;
using Griffin.ApplicationServices;
using Griffin.Container;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.ApplicationServices
{
    
    public class ApplicationServiceManagerTests
    {
        [Fact]
        public void fortsätt_även_om_en_tjänst_kastar_undantag()
        {
            var service = Substitute.For<IApplicationService>();
            service.When(x => x.Start()).Do(x => { throw new NotSupportedException(); });
            var okService = Substitute.For<IApplicationService>();
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service, okService });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            Action actual = sut.Start;

            actual.ShouldThrow<AggregateException>();
            okService.Received().Start();
        }

        [Fact]
        public void starta_inte_körande_tjänst()
        {
            var service = Substitute.For<IApplicationService>();
            service.IsRunning.Returns(true);
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.Start();

            service.DidNotReceive().Start();
        }

        [Fact]
        public void starta_inte_en_disablad_tjänst()
        {
            var service = Substitute.For<IApplicationService>();
            service.IsRunning.Returns(true);
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(false);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.Start();

            service.DidNotReceive().Start();
        }

        [Fact]
        public void start_kör_igång_timern()
        {
            var service = Substitute.For<IApplicationService>();
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.StartInterval = TimeSpan.FromMilliseconds(100);
            sut.Start();
            service.ClearReceivedCalls();
            Thread.Sleep(200);

            service.Received().Start();
        }

        [Fact]
        public void stop_stänger_av_den_regelbundna_kontrollen()
        {
            var service = Substitute.For<IApplicationService>();
            service.IsRunning.Returns(true);
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.CheckInterval = TimeSpan.FromMilliseconds(100);
            sut.StartInterval = TimeSpan.FromMilliseconds(100);
            sut.Start();
            Thread.Sleep(200);
            sut.Stop();
            service.ClearReceivedCalls();
            Thread.Sleep(200);

            var result = service.DidNotReceive().IsRunning;
        }

        [Fact]
        public void samla_fel_om_tjänster_kastar_undantag_under_nedstängning()
        {
            var service = Substitute.For<IApplicationService>();
            service.IsRunning.Returns(true);
            service.When(x => x.Stop()).Do(x => { throw new NotSupportedException(); });
            var okService = Substitute.For<IApplicationService>();
            okService.IsRunning.Returns(true);
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service, okService });
            var settingsRepos = Substitute.For<ISettingsRepository>();

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            Action actual = sut.Stop;

            actual.ShouldThrow<AggregateException>();
            okService.Received().Stop();
        }

        [Fact]
        public void anropa_inte_stop_på_nedstängd_tjänst_i_timern()
        {
            var service = Substitute.For<IApplicationService>();
            var okService = Substitute.For<IApplicationService>();
            var locator = Substitute.For<IContainer>();
            var settingsRepos = Substitute.For<ISettingsRepository>();
            service.IsRunning.Returns(false);
            locator.ResolveAll<IApplicationService>().Returns(new[] { service, okService });
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.CheckInterval = TimeSpan.FromMilliseconds(100);
            sut.StartInterval = TimeSpan.FromMilliseconds(100);
            sut.Start();
            Thread.Sleep(200);
            sut.Stop();

            okService.DidNotReceive().Stop();
        }

        [Fact]
        public void starta_nedstängd_tjänst_i_timern_om_den_är_enablad()
        {
            var started = new ManualResetEvent(false);
            var service = Substitute.For<IApplicationService>();
            var okService = Substitute.For<IApplicationService>();
            var locator = Substitute.For<IContainer>();
            var settingsRepos = Substitute.For<ISettingsRepository>();
            service.IsRunning.Returns(false);
            locator.ResolveAll<IApplicationService>().Returns(new[] { service, okService });
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);
            service.When(x => x.Start())
                .Do(x => started.Set());

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.StartInterval = TimeSpan.FromMilliseconds(0);
            sut.CheckInterval = TimeSpan.FromMilliseconds(100);
            sut.Start();
            started.WaitOne(50).Should().BeTrue();
            started.Reset();
            
            started.WaitOne(100).Should().BeTrue();
        }

        [Fact]
        public void stäng_av_tjänst_i_timern_om_den_har_blivit_disablad()
        {
            var stopped = new ManualResetEvent(false);
            var service = Substitute.For<IApplicationService>();
            var okService = Substitute.For<IApplicationService>();
            var locator = Substitute.For<IContainer>();
            var settingsRepos = Substitute.For<ISettingsRepository>();
            service.IsRunning.Returns(true);
            locator.ResolveAll<IApplicationService>().Returns(new[] { service, okService });
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);
            service.When(x => x.Stop()).Do(x => stopped.Set());

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.StartInterval = TimeSpan.FromMilliseconds(0);
            sut.CheckInterval = TimeSpan.FromMilliseconds(100);
            sut.Start();
            Thread.Sleep(40);
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(false);

            stopped.WaitOne(100).Should().BeTrue();
        }


        [Fact]
        public void trigga_händelsen_från_timern_om_en_tjänst_inte_går_att_starta()
        {
            var service = Substitute.For<IApplicationService>();
            service.IsRunning.Returns(false);
            var okService = Substitute.For<IApplicationService>();
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service, okService });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);
            ApplicationServiceFailedEventArgs actual = null;

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.CheckInterval = TimeSpan.FromMilliseconds(100);
            sut.StartInterval = TimeSpan.FromMilliseconds(100);
            sut.ServiceStartFailed += (o, e) => actual = e;
            sut.Start();
            service.When(x => x.Start()).Do(x => { throw new InvalidOperationException(); });
            Thread.Sleep(200);

            actual.Should().NotBeNull();
            actual.ApplicationService.Should().Be(service);
            actual.Exception.Should().BeOfType<InvalidOperationException>();
        }

    }
}
