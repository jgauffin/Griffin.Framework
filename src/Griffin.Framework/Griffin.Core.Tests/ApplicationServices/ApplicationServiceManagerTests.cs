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
        public void collect_all_exceptions_during_Start_before_throwing()
        {
            var service = Substitute.For<IApplicationService>();
            service.When(x => x.Start()).Do(x => { throw new NotSupportedException(); });
            var service2 = Substitute.For<IApplicationService>();
            service2.When(x => x.Start()).Do(x => { throw new NotImplementedException(); });
            var okService = Substitute.For<IApplicationService>();
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service, okService, service2 });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            Action actual = sut.Start;

            actual.ShouldThrow<AggregateException>().And.InnerExceptions.Count.Should().Be(2);
            okService.Received().Start();
        }

        [Fact]
        public void do_not_start_running_service_during_service_check()
        {
            var service = Substitute.For<IApplicationService, IGuardedService>();
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);
            ((IGuardedService)service).IsRunning.Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.CheckServices();

            service.DidNotReceive().Start();
        }

        [Fact]
        public void start_not_running_service_during_service_check()
        {
            var service = Substitute.For<IApplicationService, IGuardedService>();
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.CheckServices();

            service.Received().Start();
        }

        [Fact]
        public void stop_running_service_if_its_been_disabled()
        {
            var service = Substitute.For<IApplicationService, IGuardedService>();
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(false);
            ((IGuardedService)service).IsRunning.Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.CheckServices();

            service.Received().Stop();
        }

        [Fact]
        public void do_not_stop_running_service_if_its_enabled()
        {
            var service = Substitute.For<IApplicationService, IGuardedService>();
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);
            ((IGuardedService)service).IsRunning.Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.CheckServices();

            service.DidNotReceive().Stop();
        }

        [Fact]
        public void do_not_start_not_running_service_during_service_check_if_its_disabled()
        {
            var service = Substitute.For<IApplicationService, IGuardedService>();
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(false);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.CheckServices();

            service.DidNotReceive().Start();
        }

        [Fact]
        public void do_not_start_disabled_service()
        {
            var service = Substitute.For<IApplicationService>();
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
        public void start_enabled_service()
        {
            var service = Substitute.For<IApplicationService>();
            var locator = Substitute.For<IContainer>();
            locator.ResolveAll<IApplicationService>().Returns(new[] { service });
            var settingsRepos = Substitute.For<ISettingsRepository>();
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.Start();

            service.Received().Start();
        }

        [Fact]
        public void Start_should_activate_the_check_timer()
        {
            var service = Substitute.For<IApplicationService, IGuardedService>();
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
            var service = Substitute.For<IApplicationService, IGuardedService>();
            ((IGuardedService)service).IsRunning.Returns(true);
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

            var result = ((IGuardedService)service).DidNotReceive().IsRunning;
        }

        [Fact]
        public void collect_errors_during_shutdown_and_allow_OK_services_to_shutdown_successfully()
        {
            var service = Substitute.For<IApplicationService,IGuardedService>();
            ((IGuardedService)service).IsRunning.Returns(true);
            service.When(x => x.Stop()).Do(x => { throw new NotSupportedException(); });
            var okService = Substitute.For<IApplicationService>();
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
        public void do_not_call_stop_on_closed_services_during_check()
        {
            var service = Substitute.For<IApplicationService, IGuardedService>();
            var okService = Substitute.For<IApplicationService, IGuardedService>();
            var locator = Substitute.For<IContainer>();
            var settingsRepos = Substitute.For<ISettingsRepository>();
            ((IGuardedService)service).IsRunning.Returns(false);
            locator.ResolveAll<IApplicationService>().Returns(new[] { service, okService });
            settingsRepos.IsEnabled(Arg.Any<Type>()).Returns(true);

            var sut = new ApplicationServiceManager(locator);
            sut.Settings = settingsRepos;
            sut.CheckServices();

            okService.DidNotReceive().Stop();
        }


        [Fact]
        public void invoke_event_if_we_failed_to_start_service_during_check()
        {
            var service = Substitute.For<IApplicationService, IGuardedService>();
            ((IGuardedService)service).IsRunning.Returns(false);
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
