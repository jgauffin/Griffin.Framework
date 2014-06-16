using System;
using System.Configuration;

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
    ///         <c><![CDATA[<add key="StatisticsGenerator.Enabled.Enabled" value="true" />]]></c>
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
    ///    <add key="StatisticsGenerator.Enabled.Enabled" value="true"/>
    ///  <appSettings>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    public class AppConfigServiceSettings : ISettingsRepository
    {
        /// <summary>
        ///     Checks if a
        /// </summary>
        /// <param name="type">En typ som implementerar <see cref="IApplicationService" /></param>
        /// <returns><c>true</c> om tjänsten ska vara uppe och snurra.;<c>false</c> om ska vara nedstängd.</returns>
        public bool IsEnabled(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            return ConfigurationManager.AppSettings[type.Name + ".Enabled"] == "true";
        }
    }
}