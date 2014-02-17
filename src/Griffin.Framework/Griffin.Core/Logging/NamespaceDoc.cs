using System.Runtime.CompilerServices;
using Griffin.Logging.Providers;

namespace Griffin.Logging
{
    /// <summary>
    ///     A simple logging layer which can be used to log entries to whatever you want.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         To get started you should create a <see cref="LogProvider" /> and assign loggers to it. Finally assign it to
    ///         the <see cref="LogManager.Provider" /> to finish initialization.
    ///     </para>
    ///     <para>Now you can use the <c>GetLogger()</c> methods in your classes to access loggers.</para>
    /// </remarks>
    /// <example>
    ///     <para>To setup the logging library:</para>
    ///     <code>
    /// <![CDATA[
    /// var consoleLogger = new ConsoleLogger() { Filter = new OnlyExceptions() };
    /// var systemDebugLogger = new SystemDebugLogger() { Filter = LogLevelFilter { MinLevel = LogLevel.Info } };
    /// var logProvider = new LogProvider();
    /// logProvider.Add(consoleLogger);
    /// logProvider.Add(systemDebugLogger, new NamespaceFilter("MyApp.Core"));
    /// 
    /// LogManager.Provider = logProvider;
    /// ]]>
    /// </code>
    ///     <para>And to use it:</para>
    ///     <code>
    /// <![CDATA[
    /// public class YourClass
    /// {
    ///     ILogger _logger = LogManager.GetLogger<YourClass>();
    /// 
    ///     public void SomeMethod()
    ///     {
    ///         _logger.Info("Hello {0}", "world");
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [CompilerGenerated]
    internal class NamespaceDoc
    {
    }
}