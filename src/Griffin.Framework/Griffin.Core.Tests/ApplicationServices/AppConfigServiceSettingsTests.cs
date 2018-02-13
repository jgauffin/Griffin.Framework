using FluentAssertions;
using Griffin.ApplicationServices;
using Griffin.Configuration;
using Xunit;

namespace Griffin.Core.Tests.ApplicationServices
{
    
    public class AppConfigServiceSettingsTests
    {
        private IConfigurationReader _reader = new ConfigurationManagerReader();

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
            var sut = new AppConfigServiceSettings(_reader);

            sut.IsEnabled(typeof (DisabledServoce)).Should().BeFalse();
        }

        [Fact]
        public void enabled_service_should_Be_reported_as_enabled()
        {
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
