using FluentAssertions;
using Griffin.ApplicationServices;
using Griffin.Configuration;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.ApplicationServices
{
    
    public class AppConfigServiceSettingsTests
    {
        private IConfigurationReader _reader = Substitute.For<IConfigurationReader>();

        public class DisabledServoce
        {
            
        }
        public class EnabledService
        {
            
        }

        public class NonExistantService
        {
            
        }
        [Fact]
        public void disabled_service_should_also_be_reported_as_disabled()
        {
            _reader.ReadAppSetting("DisabledServoce.Enabled").Returns("false");

            var sut = new AppConfigServiceSettings(_reader);

            sut.IsEnabled(typeof (DisabledServoce)).Should().BeFalse();
        }

        [Fact]
        public void enabled_service_should_Be_reported_as_enabled()
        {
            _reader.ReadAppSetting("EnabledService.Enabled").Returns("true");

            var sut = new AppConfigServiceSettings(_reader);

            sut.IsEnabled(typeof(EnabledService)).Should().BeTrue();
        }

        [Fact]
        public void non_configured_service_should_be_reported_as_disabler_per_default()
        {
            var sut = new AppConfigServiceSettings(_reader);

            sut.IsEnabled(typeof(NonExistantService)).Should().BeFalse();
        }

    }
}
