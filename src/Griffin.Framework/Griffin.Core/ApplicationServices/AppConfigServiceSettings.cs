using System;
using Griffin.Configuration;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Uses <c>app.config</c> (eller <c>web.config)</c>) to identify services that should be started by Griffin Framework.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         There must exist a key per service in <c><![CDATA[<appSettings>]]></c>. It defines wether a service should be
    ///         running or not. <see cref="IApplicationService" />. The name
    ///         should be "ClassName.Enabled". For instance if you have a class named "StatisticsGenerator" the key should be
    ///         named "StatisticsGenerator.Enabled":
    ///         <c><![CDATA[<add key="StatisticsGenerator.Enabled" value="true" />]]></c>
    ///     </para>
    ///     <para>
    ///         Services which keys are not found are interpreted as being disabled.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <para>Class</para>
    ///     <code>
    /// public class StatisticsGenerator : IApplicationService
    /// {
    ///     // [...]
    /// }
    /// </code>
    ///     <para>
    ///         app.config setting:
    ///     </para>
    ///     <code>
    /// <![CDATA[
    /// <configuration>
    ///  <appSettings>
    ///    <add key="StatisticsGenerator.Enabled" value="true"/>
    ///  <appSettings>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    public class AppConfigServiceSettings : ISettingsRepository
    {
        private readonly IConfigurationReader _configReader;
#if !NET45
        public AppConfigServiceSettings(IConfigurationReader configReader)
        {
            _configReader = configReader;
        }
#else
        public AppConfigServiceSettings()
        {
            _configReader = new ConfigurationManagerReader();
        }
#endif
        /// <summary>
        ///     Check if a service/job should be running.
        /// </summary>
        /// <param name="type">
        ///     A type that implements <see cref="IApplicationService" /> or <see cref="IBackgroundJob"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the service/job should be running.;<c>false</c> if it should be shut down.
        /// </returns>
        /// <exception cref="ArgumentNullException">type</exception>
        public bool IsEnabled(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return _configReader.ReadAppSetting(type.Name + ".Enabled") == "true";
        }
    }
}
